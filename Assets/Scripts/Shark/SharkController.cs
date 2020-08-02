using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SharkController : MonoBehaviour
{
    GameObject mouth, shark;
    private Vector3 scaleChange;
    float maxScale;
    double growHp;
    double growDam;

    [Space(5)]
    [Header("Shark Stats")]
    public DamageSystem sharkStat;
    [SerializeField] private double sharkHp;
    [SerializeField] private double sharkMaxHp;
    [SerializeField] private double sharkDamage;

    [Space(5)]
    [Header("Movement")]
    [SerializeField] private float swimmingSpeedMult = 0.15f;
    [SerializeField] private float forwardShiftMaxSpeed = 2;
    [SerializeField] private float animatorVelocityWithShift = 2.2f;
    [SerializeField] private float normalToShiftVelocityTime = 5;
    [SerializeField] bool autoPitchRollAlign = true;
    [SerializeField] float autoPitchRollSpeed = 0.1f;

    [Space(5)]
    [Header("Movement with keyboard")]
    [Tooltip("All movement with keyboard, attacking with space. Use SimpleFixedCamera for better experience")]
    [SerializeField] private float yawSpeed = 1.2f;
    [SerializeField] private float rollSpeed = 1.2f;
    [SerializeField] private float pitchSpeed = 1.2f;
    [Range(1, 10)] [SerializeField] private float rotateShiftMultKeyboard = 4f;

    [Space(5)]
    [Header("Movement with mouse")]
    [SerializeField] private bool mouseRotationControlled = true;
    [Range(1, 10)] [SerializeField] private float rotateShiftMultMouse = 4f;

    int attackAnimation = 2;
    
    private Animator sharkAnim;
    private Rigidbody sharkRb;

    #region AnimatorParameters
    private float animatorVelocity = 1;
    private bool isMoving = false;
    private bool isRolling = false;
    private float forward = 0;
    private float actualForward = 0;
    private float yaw = 0;
    private float pitch = 0;
    private float roll = 0;
    #endregion

    #region Animator_HashTag
    private static int doIdleHash = Animator.StringToHash("doIdle");
    private static int idleRandomHash = Animator.StringToHash("idleRandom");
    private static int isMovingHash = Animator.StringToHash("isMoving");
    private static int isRollingHash = Animator.StringToHash("isRolling");
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isAttackingTriggerHash = Animator.StringToHash("isAttackingTrigger");
    private static int attackIndexHash = Animator.StringToHash("attackIndex");
    private static int velocityHash = Animator.StringToHash("velocity");
    private static int yawHash = Animator.StringToHash("direction");
    private static int pitchHash = Animator.StringToHash("pitch");
    private static int rollHash = Animator.StringToHash("roll");
    #endregion

    void Start ()
    {
        shark = GameObject.Find("WhiteShark");
        scaleChange = new Vector3(0.00001f, 0.00001f, 0.00001f);
        maxScale = 5.0f;
        growHp = 85.0;
        growDam = 12.0;
        mouth = GameObject.Find("MouthBox");
        mouth.SetActive(false);
        sharkMaxHp = 10;
        sharkHp = sharkMaxHp;
        sharkDamage = .5;
        sharkStat = new DamageSystem(sharkMaxHp, sharkDamage);
        sharkAnim = GetComponent<Animator>();
        sharkRb = GetComponent<Rigidbody>();

        sharkRb.useGravity = false;
        sharkRb.drag = 1f;
        sharkRb.angularDrag = 1f;
        sharkRb.constraints = RigidbodyConstraints.FreezeRotationZ;
}
	
	void Update ()
    {
        AnimatorStateMachine();
        Attacking();
        Growth();
        sharkStat.setHp(sharkMaxHp);
        sharkStat.setDam(sharkDamage);
        sharkHp = sharkStat.getHp();

        //test shark damage
        if (Input.GetKeyUp(KeyCode.F))
        {
            sharkStat.damage(1.0);
        }
    }
    private void Growth()
    {
        if (shark.transform.localScale.x < maxScale)
            shark.transform.localScale += scaleChange;
        if (sharkMaxHp < growHp)
        {
            sharkMaxHp += .00001;
            sharkStat.heal(.00001);
        }
            
        if (sharkDamage < growDam)
            sharkDamage += .00001;
    }
    private void AnimatorStateMachine()
    {
        forward = Input.GetAxis("Vertical");
        if (!mouseRotationControlled)
        {
            yaw = Input.GetAxis("Horizontal") * 0.4f;
            pitch = Input.GetAxis("Pitch") * 0.4f;
            roll = Input.GetAxis("Roll") * 0.4f;

            sharkAnim.SetFloat(yawHash, yaw);
            sharkAnim.SetFloat(pitchHash, pitch);
        }
        else
        {
            yaw += Input.GetAxis("Mouse X") * 0.1f;
            pitch -= Input.GetAxis("Mouse Y") * 0.1f;
            yaw = Mathf.Clamp(yaw, -1f, 1f);
            pitch = Mathf.Clamp(pitch, -1f, 1f);

            sharkAnim.SetFloat(yawHash, yaw);
            sharkAnim.SetFloat(pitchHash, pitch);
        }

        if (rollSpeed > 0) sharkAnim.SetFloat(rollHash, roll);

        if (forward > 0.1 || forward < -0.1 || yaw > 0.1 || yaw < -0.1 || pitch > 0.1 || pitch < -0.1)
        {
            isMoving = true;
            sharkAnim.SetBool(isMovingHash, true);
        }
        else
        {
            isMoving = false;
            sharkAnim.SetBool(isMovingHash, false);
        }

        if ( rollSpeed > 0 && (roll > 0.1 || roll < -0.1))
        {
            isRolling = true;
            sharkAnim.SetBool(isRollingHash, true);
        }
        else
        {
            isRolling = false;
            sharkAnim.SetBool(isRollingHash, false);
        }

        if (Input.GetKey(KeyCode.LeftShift) && (forward > 0.1f || forward < -0.1f))
        {
            animatorVelocity = Mathf.Lerp(animatorVelocity, animatorVelocityWithShift, normalToShiftVelocityTime * Time.deltaTime);
            actualForward = Mathf.Lerp(actualForward, forwardShiftMaxSpeed, normalToShiftVelocityTime * Time.deltaTime);
        }
        else
        {
            animatorVelocity = Mathf.Lerp(animatorVelocity, 1, normalToShiftVelocityTime * Time.deltaTime);
            actualForward = Mathf.Lerp(actualForward, forward, normalToShiftVelocityTime);
        }
        sharkAnim.SetFloat(velocityHash, animatorVelocity);
    }

    bool reset = false;
    private IEnumerator shake;

    private void Attacking()
    {
        shake = ShakeTimer(.1f);
        if (Input.GetMouseButtonDown(0))
        {
            int attackIndex = UnityEngine.Random.Range(0, attackAnimation + 1);
            sharkAnim.SetTrigger(isAttackingTriggerHash);
            sharkAnim.SetInteger(attackIndexHash, attackIndex);
            StartCoroutine(AttackTimer(0.25f));
        }

        if (Input.GetMouseButton(0))
        {
            if (!reset)
            {
                if (Input.GetMouseButton(0))
                    StartCoroutine(shake);
                reset = true;
            }
        }
        else
        {
            StopAllCoroutines();
            mouth.SetActive(false);
            sharkAnim.SetBool(isAttackingHash, false);
            reset = false;
        }

        IEnumerator AttackTimer(float delay)
        {
            mouth.SetActive(true);
            yield return new WaitForSeconds(delay);
            mouth.SetActive(false);
        }
        IEnumerator ShakeTimer(float delay)
        {
            sharkAnim.SetBool(isAttackingHash, true);
            yield return new WaitForSeconds(.5f);
            mouth.SetActive(true);
            yield return new WaitForSeconds(.5f);
            mouth.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mouth.SetActive(true);
            yield return new WaitForSeconds(.1f);
            mouth.SetActive(false);
            yield return new WaitForSeconds(.1f);
            mouth.SetActive(true);
            yield return new WaitForSeconds(.1f);
            mouth.SetActive(false);
            yield return new WaitForSeconds(.1f);
            mouth.SetActive(true);
            yield return new WaitForSeconds(.1f);
            mouth.SetActive(false);

            yield return new WaitForSeconds(delay);
            sharkAnim.SetBool(isAttackingHash, false);
        }
    }

    private void FixedUpdate()
    {
        AutoXZalign();
        Locomotion();
    }

    private void AutoXZalign()
    {
        if (autoPitchRollAlign && !isMoving)
        {
            Vector3 xzAlignVector = new Vector3(0, transform.rotation.eulerAngles.y, 0);
            Quaternion desireRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(xzAlignVector), autoPitchRollSpeed);
            sharkRb.MoveRotation(desireRotation);
        }
    }

    private void Locomotion()
    {
        if (isMoving || isRolling)
        {
            Vector3 forwardMovement = transform.position + transform.forward * actualForward * swimmingSpeedMult;
            sharkRb.MovePosition(forwardMovement);
            Vector3 deltaRotation;

            if (!mouseRotationControlled)
            {
                deltaRotation = new Vector3(pitch * pitchSpeed, yaw * yawSpeed, -roll * rollSpeed) * rotateShiftMultKeyboard;
                sharkRb.MoveRotation(sharkRb.rotation * Quaternion.Euler(deltaRotation));
            }
            else
            {
                deltaRotation = new Vector3(pitch, yaw, 0) * rotateShiftMultMouse;

                Quaternion desireRotation = sharkRb.rotation * Quaternion.Euler(deltaRotation);
                desireRotation = Quaternion.Euler(new Vector3(desireRotation.eulerAngles.x, desireRotation.eulerAngles.y, 0));
                sharkRb.MoveRotation(Quaternion.Lerp(sharkRb.rotation, desireRotation, 0.6f));

                if (sharkRb.rotation.eulerAngles.x >= 45 && sharkRb.rotation.eulerAngles.x <180)
                    sharkRb.transform.rotation = Quaternion.Euler(new Vector3(45, sharkRb.rotation.eulerAngles.y, sharkRb.rotation.eulerAngles.z));
                if (sharkRb.rotation.eulerAngles.x >= 180 && sharkRb.rotation.eulerAngles.x <= 315)
                    sharkRb.transform.rotation = Quaternion.Euler(new Vector3(315, sharkRb.rotation.eulerAngles.y, sharkRb.rotation.eulerAngles.z));
            }
        }
        else
            sharkRb.velocity = Vector3.Lerp(sharkRb.velocity, Vector3.zero, Time.deltaTime * 1f);
    }
}
