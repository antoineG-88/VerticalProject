﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapplingHandler : MonoBehaviour
{
    [Header("Grappling settings")]
    public bool instantaneousAttach;
    public float shootForce;
    public float ropeLength;
    public float shootCooldown;
    public float releasingHookDist;
    public bool useGhostHook;
    public float minAttachDistance;
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
                        hit = Physics2D.Raycast(shootPoint.position, direction, ropeLength, LayerMask.GetMask("Ring", "Ground", "Enemy"));
                        if (hit && (hit.collider.CompareTag("Ring") || hit.collider.CompareTag("Enemy")) && nearestRing != hit.collider.gameObject && Vector2.Angle(direction, new Vector2(aimDirection.x, -aimDirection.y)) < minAngleFound)
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
                    if(useGhostHook)
                    {
                        isGhostHook = true;
                    }

                    if (useAutoAim && instantaneousAttach)
                    {
                        if(nearestRing != null)
                        {
                            currentHook = Instantiate(hookPrefab, nearestRing.transform.position, Quaternion.identity);
                            AttachHook(nearestRing);
                            hookRb = currentHook.GetComponent<Rigidbody2D>();
                        }
                        else
                        {
                            // No Target Feedback
                        }
                    }
                    else
                    {
                        currentHook = Instantiate(hookPrefab, shootPoint.position, Quaternion.identity);
                        hookRb = currentHook.GetComponent<Rigidbody2D>();
                        hookRb.velocity = shootDirection * shootForce;
                    }
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

                rb.velocity += tractionDirection * tractionForce * Time.deltaTime;

                tractionDragForce = tractionAirDensity * Mathf.Pow(rb.velocity.magnitude, 2) / 2;
                rb.velocity -= rb.velocity * tractionDragForce * Time.deltaTime;

                if (rb.velocity.magnitude > maxTractionSpeed)
                {
                    rb.velocity = tractionDirection * maxTractionSpeed;
                }

                isTracting = true;
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
            if(attachedObject == null)
            {
                currentHook.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Atan2(hookRb.velocity.y, -hookRb.velocity.x) * 180 / Mathf.PI);
            }

            if(Vector2.Distance(transform.position, currentHook.transform.position) > ropeLength + 2)
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
