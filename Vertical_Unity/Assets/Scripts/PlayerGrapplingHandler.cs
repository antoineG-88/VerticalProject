using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapplingHandler : MonoBehaviour
{
    [Header("Grappling settings")]
    public bool useRealisticMomentum;
    public float shootForce;
    public float ropeLength;
    public float shootCooldown;
    public float tractionForce;
    public float releasingHookDist;
    public float tractionAirDensity;
    public float minAttachDistance;
    [Range(0,100)] public float velocityKeptReleasingHook;
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
    [Header("Debug settings")]
    public Color ghostHookColor;
    
    private Rigidbody2D rb;
    private LineRenderer ropeRenderer;
    private Vector2 aimDirection;
    private bool isAiming;
    private GameObject currentHook;
    private Rigidbody2D hookRb;
    private float timeBeforeNextShoot;
    private GameObject nearestRing;
    private float subwidthAimAngle;
    private float firstAngle;
    private Vector2 shootDirection;
    private bool shootFlag;
    private bool isHooked;
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
        ropeRenderer = GetComponent<LineRenderer>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        distanceJoint.enabled = false;
        ringHighLighterO.SetActive(false);
        isAiming = false;
        shootFlag = true;
        isHooked = false;
        isTracting = false;
        canUseTraction = true;
        canShoot = true;
        basicHookColor = hookPrefab.GetComponent<SpriteRenderer>().color;
        armShoulderO.SetActive(false);
        timeBeforeNextShoot = 0;
        subwidthAimAngle = widthAimAngle / (raycastNumber - 1);
        firstAngle = -widthAimAngle / 2;
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (currentHook != null && attachedObject == null)
        {
            currentHook.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(hookRb.velocity.y, -hookRb.velocity.x) * 180 / Mathf.PI);
        }

        ShootManager();

        TractionManager();

        HookManager();
    }

    void ShootManager()
    {
        if(canShoot)
        {
            if (!isAiming && (Mathf.Abs(Input.GetAxisRaw("RJoystickH")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("RJoystickV")) > 0.1f))
            {
                isAiming = true;
                armShoulderO.SetActive(true);
            }
            else if (isAiming && Mathf.Abs(Input.GetAxisRaw("RJoystickH")) <= 0.1f && Mathf.Abs(Input.GetAxisRaw("RJoystickV")) <= 0.1f)
            {
                isAiming = false;
                armShoulderO.SetActive(false);
            }

            if (timeBeforeNextShoot > 0)
            {
                timeBeforeNextShoot -= Time.deltaTime;
            }
            else
            {
                timeBeforeNextShoot = 0;
            }

            if (isAiming)
            {
                aimDirection = new Vector2(Input.GetAxis("RJoystickH"), Input.GetAxis("RJoystickV")).normalized;
                armShoulderO.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(aimDirection.x, aimDirection.y) * 180 / Mathf.PI - 90);

                if (useAutoAim)
                {
                    RaycastHit2D hit;
                    float minAngleFound = widthAimAngle;
                    nearestRing = null;
                    for (int i = 0; i < raycastNumber; i++)
                    {
                        float relativeAngle = firstAngle + subwidthAimAngle * i;
                        float angledDirection = (Mathf.Atan2(aimDirection.x, aimDirection.y) * 180 / Mathf.PI - 90) + relativeAngle;

                        Vector2 direction = new Vector2(Mathf.Cos((angledDirection) * Mathf.PI / 180), Mathf.Sin((angledDirection) * Mathf.PI / 180));
                        hit = Physics2D.Raycast(shootPoint.position, direction, ropeLength, LayerMask.GetMask("Ring", "Walkable", "Ennemy"));

                        if (hit && (hit.collider.CompareTag("Ring") || hit.collider.CompareTag("Ennemy")) && nearestRing != hit.collider.gameObject && Vector2.Angle(direction, new Vector2(aimDirection.x, -aimDirection.y)) < minAngleFound)
                        {
                            nearestRing = hit.collider.gameObject;
                            minAngleFound = Vector2.Angle(direction, new Vector2(aimDirection.x, -aimDirection.y));
                        }
                    }

                    if (nearestRing != null)
                    {
                        ringHighLighterO.SetActive(true);
                        ringHighLighterO.transform.position = nearestRing.transform.position;
                        shootDirection = new Vector2(nearestRing.transform.position.x - shootPoint.position.x, nearestRing.transform.position.y - shootPoint.position.y).normalized;
                    }
                    else
                    {
                        shootDirection = aimDirection;
                        shootDirection.y *= -1;
                        ringHighLighterO.SetActive(false);
                    }
                }
                else
                {
                    shootDirection = aimDirection;
                    shootDirection.y *= -1;
                }


                if (shootFlag && Input.GetAxis("RTAxis") > 0.1f && timeBeforeNextShoot <= 0 && !isHooked)
                {
                    shootFlag = false;
                    ReleaseHook();
                    currentHook = Instantiate(hookPrefab, shootPoint.position, Quaternion.identity);
                    isGhostHook = true;
                    hookRb = currentHook.GetComponent<Rigidbody2D>();
                    hookRb.velocity = shootDirection * shootForce;
                }
                else if (!shootFlag && Input.GetAxisRaw("RTAxis") == 0)
                {
                    shootFlag = true;
                    timeBeforeNextShoot = shootCooldown;
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
            currentHook.transform.position = attachedObject.transform.position;
            hookRb.velocity = Vector2.zero;
            currentHook.GetComponent<Collider2D>().isTrigger = true;

            ropeRenderer.enabled = true;
            Vector3[] ropePoints = new Vector3[2];
            ropePoints[0] = transform.position;
            ropePoints[1] = currentHook.transform.position;
            ropeRenderer.SetPositions(ropePoints);

            tractionDirection = (currentHook.transform.position - transform.position).normalized;

            if (canUseTraction && Input.GetAxisRaw("RTAxis") == 1)
            {
                isTracting = true;
                GameData.playerMovement.inControl = false;
                GameData.playerMovement.isAffectedbyGravity = false;
                rb.velocity += tractionDirection * tractionForce;
                tractionDragForce = tractionAirDensity * Mathf.Pow(rb.velocity.magnitude, 1.2f) / 2;
                rb.velocity -= rb.velocity * tractionDragForce * Time.deltaTime;
            }

            if(isTracting && (Input.GetAxisRaw("RTAxis") == 0 || (Vector2.Distance(transform.position, currentHook.transform.position) < releasingHookDist)))
            {
                rb.velocity *= velocityKeptReleasingHook / 100;
                isTracting = false;
                ReleaseHook();
            }
        }
        else
        {
            ropeRenderer.enabled = false;
        }
    }

    private void HookManager()
    {
        if(isHooked)
        {
            RaycastHit2D ringHit = Physics2D.Raycast(transform.position, tractionDirection, ropeLength, LayerMask.GetMask("Ring", "Ennemy", "Walkable"));
            if (ringHit && !ringHit.collider.CompareTag("Ring") && !ringHit.collider.CompareTag("Ennemy"))
            {
                Debug.Log("Object touched : " + ringHit.transform.gameObject.name);
                BreakRope();
            }
        }

        if(currentHook != null)
        {
            if(Vector2.Distance(transform.position, currentHook.transform.position) > ropeLength + 2)
            {
                BreakRope();
            }

            if(!isHooked && Physics2D.OverlapCircle(currentHook.transform.position, 0.2f, LayerMask.GetMask("Walkable")))
            {
                ReleaseHook();
            }

            if(isGhostHook)
            {
                currentHook.GetComponent<SpriteRenderer>().color = ghostHookColor;
                if(Vector2.Distance(currentHook.transform.position, transform.position) > minAttachDistance)
                {
                    currentHook.GetComponent<SpriteRenderer>().color = basicHookColor;
                    isGhostHook = false;
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
            distanceJoint.enabled = true;
            distanceJoint.connectedBody = attachPos.GetComponent<Rigidbody2D>();
            distanceJoint.distance = Vector2.Distance(attachPos.transform.position, transform.position);
        }
    }

    public void ReleaseHook()
    {
        isHooked = false;
        isTracting = false;
        ropeRenderer.enabled = false;
        GameData.playerMovement.inControl = true;
        attachedObject = null;
        GameData.playerAttackManager.kickUsed = false;
        GameData.playerMovement.isAffectedbyGravity = true;
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
