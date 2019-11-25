using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapplingHandler : MonoBehaviour
{
    [Header("Grappling settings")]
    public bool instantaneousAttach;
    public float shootForce;
    public float ropeLength;
    public float ropeAttachLengthDifference;
    public float shootCooldown;
    public float releasingHookDist;
    public bool useGhostHook;
    public float minAttachDistance;
    public bool keepAim;
    public bool useJoint;
    [Space]
    [Header("Momentum settings")]
    public bool resetMomentumOnTraction;
    public float tractionForce;
    public float tractionAirDensity;
    public float maxTractionSpeed;
    [Range(1,3)] public float momentumAmplification;
    public float startTractionPropulsion;
    [Space]
    [Range(0,100)] public float velocityKeptReleasingHook;
    [Space]
    [Header("AutoAim settings")]
    public bool useAutoAim;
    public float widthAimAngle;
    public float raycastNumber;
    [Space]
    [Header("Grappling References")]
    public GameObject armShoulderO;
    public Transform shootPoint;
    public GameObject hookPrefab;
    public GameObject ringHighLighterO;
    public GameObject grapplingStartPointO;
    [Header("Debug settings")]
    public Color ghostHookColor;
    public bool displayAutoAimRaycast;
    
    private Rigidbody2D rb;
    private Vector2 aimDirection;
    private LineRenderer ropeRenderer;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public GameObject currentHook;
    private Rigidbody2D hookRb;
    private float timeBeforeNextShoot;
    private GameObject selectedRing;
    private float subwidthAimAngle;
    private float firstAngle;
    private Vector2 shootDirection;
    [HideInInspector] public bool isHooked;
    private float tractionDragForce;
    private DistanceJoint2D distanceJoint;
    private bool isGhostHook;
    private Color basicHookColor;
    [HideInInspector] public GameObject attachedObject;
    [HideInInspector] public Vector2 tractionDirection;
    [HideInInspector] public bool isTracting;
    [HideInInspector] public bool canUseTraction;
    [HideInInspector] public bool canShoot;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        ropeRenderer = GetComponent<LineRenderer>();
        distanceJoint.enabled = false;
        ringHighLighterO.SetActive(false);
        isAiming = false;
        isHooked = false;
        isTracting = false;
        canUseTraction = true;
        canShoot = true;
        isGhostHook = false;
        basicHookColor = hookPrefab.GetComponent<SpriteRenderer>().color;
        armShoulderO.SetActive(false);
        timeBeforeNextShoot = 0;
        subwidthAimAngle = widthAimAngle / (raycastNumber - 1);
        firstAngle = -widthAimAngle / 2;
    }

    private void Update()
    {
        ShootManager();
    }

    private void FixedUpdate()
    {
        TractionManager();

        HookManager();
    }

    void ShootManager()
    {

        if (timeBeforeNextShoot > 0)
        {
            timeBeforeNextShoot -= Time.deltaTime;
        }

        if (canShoot)
        {
            if (!isAiming && (Mathf.Abs(GameData.gameController.input.rightJoystickHorizontal) > 0.1f || Mathf.Abs(GameData.gameController.input.rightJoystickVertical) > 0.1f))
            {
                isAiming = true;
                armShoulderO.SetActive(true);
            }
            else if (isAiming && Mathf.Abs(GameData.gameController.input.rightJoystickHorizontal) <= 0.1f && Mathf.Abs(GameData.gameController.input.rightJoystickVertical) <= 0.1f)
            {
                isAiming = false;
                if(!keepAim)
                {
                    armShoulderO.SetActive(false);
                }
            }

            if (isAiming || keepAim)
            {
                if(isAiming)
                {
                    aimDirection = new Vector2(GameData.gameController.input.rightJoystickHorizontal, -GameData.gameController.input.rightJoystickVertical).normalized;
                    armShoulderO.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(aimDirection.x, aimDirection.y) * 180 / Mathf.PI - 90);
                }


                selectedRing = null;

                if (useAutoAim)
                {
                    RaycastHit2D hit;
                    float minAngleFound = widthAimAngle;
                    for (int i = 0; i < raycastNumber; i++)
                    {
                        float relativeAngle = firstAngle + subwidthAimAngle * i;
                        float angledDirection = (Mathf.Atan2(aimDirection.x, aimDirection.y) * 180 / Mathf.PI - 90) + relativeAngle;
                        Vector2 direction = new Vector2(Mathf.Cos((angledDirection) * Mathf.PI / 180), Mathf.Sin((angledDirection) * Mathf.PI / 180));
                        Vector2 raycastOrigin = shootPoint.position;
                        if (instantaneousAttach && useGhostHook)
                        {
                            raycastOrigin = (Vector2)transform.position + direction * minAttachDistance;

                            if (displayAutoAimRaycast)
                            {
                                Debug.DrawRay(raycastOrigin, direction * ropeLength, Color.cyan);
                            }
                        }
                        hit = Physics2D.Raycast(raycastOrigin, direction, ropeLength, LayerMask.GetMask("Ring", "Ground", "Enemy"));
                        if (hit && (hit.collider.CompareTag("Ring") || hit.collider.CompareTag("Enemy")) && selectedRing != hit.collider.gameObject && Vector2.Angle(direction, new Vector2(aimDirection.x, -aimDirection.y)) < minAngleFound)
                        {
                            selectedRing = hit.collider.gameObject;
                            minAngleFound = Vector2.Angle(direction, new Vector2(aimDirection.x, -aimDirection.y));
                        }
                    }
                }
                else
                {
                    if(instantaneousAttach)
                    {
                        ropeRenderer.enabled = true;
                        Vector2 raycastOrigin = shootPoint.position;
                        if(useGhostHook)
                        {
                            raycastOrigin = (Vector2)transform.position + new Vector2(aimDirection.x, -aimDirection.y).normalized * minAttachDistance;
                        }

                        Vector3[] rayPoints = new Vector3[2];
                        rayPoints[0] = raycastOrigin;
                        rayPoints[1] = (new Vector2(aimDirection.x, -aimDirection.y) * ropeLength) + raycastOrigin;

                        RaycastHit2D grappleRay = Physics2D.Raycast(raycastOrigin, new Vector2(aimDirection.x, -aimDirection.y), ropeLength, LayerMask.GetMask("Ring", "Ground", "Enemy"));
                        if(grappleRay)
                        {
                            rayPoints[1] = grappleRay.point;
                        }

                        ropeRenderer.SetPositions(rayPoints);

                        if (grappleRay && (grappleRay.collider.CompareTag("Ring") || grappleRay.collider.CompareTag("Enemy")))
                        {
                            selectedRing = grappleRay.collider.gameObject;
                        }
                    }
                }

                if (selectedRing != null)
                {
                    ringHighLighterO.SetActive(true);
                    ringHighLighterO.transform.position = selectedRing.transform.position;
                    shootDirection = new Vector2(selectedRing.transform.position.x - shootPoint.position.x, selectedRing.transform.position.y - shootPoint.position.y).normalized;
                }
                else
                {
                    shootDirection = aimDirection;
                    shootDirection.y *= -1;
                    ringHighLighterO.SetActive(false);
                }

                if (GameData.gameController.input.rightTriggerDown && timeBeforeNextShoot <= 0 && !isHooked)
                {
                    timeBeforeNextShoot = shootCooldown;
                    ReleaseHook();

                    if (instantaneousAttach)
                    {
                        if (selectedRing != null)
                        {
                            currentHook = Instantiate(hookPrefab, selectedRing.transform.position, Quaternion.identity);
                            AttachHook(selectedRing);
                            hookRb = currentHook.GetComponent<Rigidbody2D>();
                        }
                        else
                        {
                            // No Target Feedback
                        }
                    }
                    else
                    {
                        if (useGhostHook)
                        {
                            isGhostHook = true;
                        }
                        currentHook = Instantiate(hookPrefab, shootPoint.position, Quaternion.identity);
                        hookRb = currentHook.GetComponent<Rigidbody2D>();
                        hookRb.velocity = shootDirection * shootForce;
                    }
                }
            }
            else
            {
                ringHighLighterO.SetActive(false);
            }
        }
    }

    void TractionManager()
    {
        if(isHooked)
        {
            canShoot = false;
            currentHook.transform.position = attachedObject.transform.position;
            hookRb.velocity = Vector2.zero;
            currentHook.GetComponent<Collider2D>().isTrigger = true;
            ringHighLighterO.SetActive(false);

            ropeRenderer.enabled = true;
            Vector3[] ropePoints = new Vector3[2];
            ropePoints[0] = transform.position;
            ropePoints[1] = currentHook.transform.position;
            ropeRenderer.SetPositions(ropePoints);

            tractionDirection = (currentHook.transform.position - transform.position).normalized;

            if (canUseTraction && GameData.gameController.input.rightTriggerAxis == 1)
            {
                GameData.playerMovement.isAffectedbyGravity = false;
                GameData.playerMovement.inControl = false;

                if (!isTracting)
                {
                    if(resetMomentumOnTraction)
                    {
                        float startTractionVelocity = rb.velocity.magnitude * Mathf.Cos(Mathf.Pow(Mathf.Deg2Rad * Vector2.Angle(rb.velocity, tractionDirection), momentumAmplification));

                        if (startTractionVelocity < 0)
                        {
                            startTractionVelocity = 0;
                        }

                        startTractionVelocity += startTractionPropulsion;

                        rb.velocity = startTractionVelocity * tractionDirection;
                    }
                    else
                    {
                        rb.velocity += startTractionPropulsion * tractionDirection;
                    }
                }

                if(resetMomentumOnTraction)
                {
                    float tractionDirectionAngle = Mathf.Atan(tractionDirection.y / tractionDirection.x);
                    if(tractionDirection.x < 0)
                    {
                        tractionDirectionAngle += Mathf.PI;
                    }
                    else if(tractionDirection.y < 0 && tractionDirection.x > 0)
                    {

                        tractionDirectionAngle += 2 * Mathf.PI;
                    }
                    rb.velocity = new Vector2(rb.velocity.magnitude * Mathf.Cos(tractionDirectionAngle), rb.velocity.magnitude * Mathf.Sin(tractionDirectionAngle));
                }

                rb.velocity += tractionDirection * tractionForce * Time.fixedDeltaTime;

                tractionDragForce = tractionAirDensity * Mathf.Pow(rb.velocity.magnitude, 2) / 2;
                rb.velocity -= rb.velocity * tractionDragForce * Time.fixedDeltaTime;

                if (rb.velocity.magnitude > maxTractionSpeed)
                {
                    rb.velocity = tractionDirection * maxTractionSpeed;
                }

                isTracting = true;
            }

            if(isTracting && (GameData.gameController.input.rightTriggerAxis == 0 || (Vector2.Distance(transform.position, currentHook.transform.position) < releasingHookDist)))
            {
                rb.velocity *= velocityKeptReleasingHook / 100;
                isTracting = false;

                ReleaseHook();
            }
        }
        else
        {
            canShoot = true;
            ropeRenderer.enabled = false;
        }
    }

    private void HookManager()
    {
        if (isHooked)
        {
            RaycastHit2D ringHit = Physics2D.Raycast(transform.position, tractionDirection, ropeLength, LayerMask.GetMask("Ring", "Enemy", "Ground"));
            if (ringHit && !ringHit.collider.CompareTag("Ring") && !ringHit.collider.CompareTag("Enemy"))
            {
                BreakRope();
            }
        }

        if(currentHook != null)
        {
            if(!isHooked)
            {
                currentHook.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(hookRb.velocity.y, -hookRb.velocity.x) * 180 / Mathf.PI);
            }

            if(Vector2.Distance(transform.position, currentHook.transform.position) > ropeLength + ropeAttachLengthDifference)
            {
                BreakRope();
            }

            if(!isHooked && Physics2D.OverlapCircle(currentHook.transform.position, 0.2f, LayerMask.GetMask("Ground")))
            {
                ReleaseHook();
            }

            if(useGhostHook)
            {
                if (isGhostHook)
                {
                    currentHook.GetComponent<SpriteRenderer>().color = ghostHookColor;
                    if (Vector2.Distance(currentHook.transform.position, transform.position) > minAttachDistance)
                    {
                        currentHook.GetComponent<SpriteRenderer>().color = basicHookColor;
                        isGhostHook = false;
                    }
                }
            }
        }
    }

    public void AttachHook(GameObject attachPos)
    {
        if(!isGhostHook)
        {
            isHooked = true;
            attachedObject = attachPos;

            if(!useGhostHook)
            {
                if (attachedObject.CompareTag("Enemy") && Vector2.Distance(currentHook.transform.position, transform.position) < minAttachDistance)
                {
                    BreakRope();
                }
            }

            if(useJoint)
            {
                distanceJoint.enabled = true;
                distanceJoint.connectedBody = attachPos.GetComponent<Rigidbody2D>();
                distanceJoint.distance = Vector2.Distance(attachPos.transform.position, transform.position);
            }
        }
    }

    public void ReleaseHook()
    {
        isHooked = false;
        isTracting = false;
        ropeRenderer.enabled = false;
        GameData.playerMovement.inControl = true;
        attachedObject = null;
        GameData.playerMovement.isAffectedbyGravity = true;
        GameData.playerAttackManager.isKicking = false;
        distanceJoint.enabled = false;
        if(currentHook != null)
        {
            Destroy(currentHook);
        }
    }

    public void BreakRope()
    {
        //breakingAnimation
        ReleaseHook();
    }
}
