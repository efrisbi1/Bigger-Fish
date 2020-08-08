using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SharkController : MonoBehaviour
{
    public NPCcontrol npc;
    public BigNPC bnpc;
    public patrolNPC pnpc;
    public BossNPC Bnpc;
    GameObject mouth, shark,cross,healthUI,hungerUI;
    private RawImage rawHp, rawHung;
    private Vector3 scaleChange;
    float maxScale,scalePer;
    double growHp,growDam,perD;
    string percentStr;
    Scene scene;

    [Space(5)]
    [Header("Shark Stats")]
    public DamageSystem sharkStat;
    [SerializeField] private double sharkHp;
    [SerializeField] private double sharkMaxHp;
    [SerializeField] private double sharkDamage;
    [SerializeField] private double energy;
    [SerializeField] private int factor;

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

    [SerializeField]public Texture hp0, hp1, hp2, hp3, hun0, hun1, hun2, hun3;
    [SerializeField] public GameObject hitmark, feedText,death,percent;

    int attackAnimation = 2;
    
    private Animator sharkAnim;
    private Rigidbody sharkRb;
    AudioSource aud;

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
        scalePer = 0.0f;
        Cursor.visible = false;
        scene = SceneManager.GetActiveScene();
        aud = GetComponent<AudioSource>();
        shark = GameObject.Find("WhiteShark");
        maxScale = 3.0f;
        growHp = 80.0;
        growDam = 36.0;
        energy = 2.0;
        mouth = GameObject.Find("MouthBox");
        mouth.SetActive(false);
        sharkMaxHp = 10;
        sharkHp = sharkMaxHp;
        sharkDamage = .5;
        sharkStat = new DamageSystem(sharkMaxHp, sharkDamage);
        sharkAnim = GetComponent<Animator>();
        sharkRb = GetComponent<Rigidbody>();
        cross = GameObject.Find("Cross");
        healthUI= GameObject.Find("Health");
        hungerUI= GameObject.Find("Hunger");
        rawHp = (RawImage)healthUI.GetComponent<RawImage>();
        rawHp.texture = hp3;
        rawHung = (RawImage)hungerUI.GetComponent<RawImage>();
        death.SetActive(false);

        sharkRb.useGravity = false;
        sharkRb.drag = 1f;
        sharkRb.angularDrag = 1f;
        sharkRb.constraints = RigidbodyConstraints.FreezeRotationZ;
}
	
	void Update ()
    {
        scalePer = (shark.transform.localScale.x) / maxScale;
        perD= System.Math.Round(scalePer, 4)*100;
        percentStr = perD.ToString() + "%";
        AnimatorStateMachine();
        Attacking();
        Growth();
        Hunger();
        sharkStat.setHp(sharkMaxHp);
        sharkStat.setDam(sharkDamage);
        sharkHp = sharkStat.getHp();
        UpdateUI();

        if(sharkStat.getHp()==0.0 ||sharkStat.getHp()<0.05)
        {
            Death();
        }
    }

    void Death()
    {
        death.SetActive(true);
        energy = 0.0;

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(scene.name);
    }
    private void Hunger()
    {
        if (energy > 3.0)
        {
            energy = 3.0;
        }
        if (energy < 0.0)
        {
            energy = 0.0;
        }
        if (energy == 0.0)
        {
            sharkStat.damage(0.0010);
            factor = 0;
        }
        if (energy > 2.5)
        {
            factor = 3;
            energy -= .0005;
        }
        if (energy>1.5 && energy<2.5)
        {
            factor = 2;
            energy -= .00020;
        }
        if (energy > 0.0 && energy < 1.5)
        {
            factor = 1;
            energy -= .00010;
        }
    }
    private void Growth()
    {
        if (factor == 1)
        {
            scaleChange = new Vector3(0.00002f, 0.00002f, 0.00002f);
            if (shark.transform.localScale.x < maxScale)
                shark.transform.localScale += scaleChange;
            if (sharkMaxHp < growHp)
            {
                sharkMaxHp += .00005;
                sharkStat.heal(.00005);
            }

            if (sharkDamage < growDam)
                sharkDamage += .00005;
        }
        if (factor == 2)
        {
            scaleChange = new Vector3(0.0001f, 0.0001f, 0.0001f);
            if (shark.transform.localScale.x < maxScale)
                shark.transform.localScale += scaleChange;
            if (sharkMaxHp < growHp)
            {
                sharkMaxHp += .0005;
                sharkStat.heal(.0005);
            }

            if (sharkDamage < growDam)
                sharkDamage += .00005;
        }
        if (factor == 3)
        {
            scaleChange = new Vector3(0.0002f, 0.0002f, 0.0002f);
            if (shark.transform.localScale.x < maxScale)
                shark.transform.localScale += scaleChange;
            if (sharkMaxHp < growHp)
            {
                sharkMaxHp += .005;
                sharkStat.heal(.005);
            }

            if (sharkDamage < growDam)
                sharkDamage += .0025;
        }
    }
    private void UpdateUI()
    {
        percent.GetComponent<UnityEngine.UI.Text>().text = percentStr;
        if(sharkHp>sharkMaxHp*.9)
            rawHp.texture = hp3;
        else if(sharkHp<(sharkMaxHp*.66)&&sharkHp>(sharkMaxHp*.33))
            rawHp.texture = hp2;
        else if (sharkHp < (sharkMaxHp * .33) && sharkHp > (sharkMaxHp * .1))
            rawHp.texture = hp1;
        else if (sharkHp < (sharkMaxHp*.1))
            rawHp.texture = hp0;

        if (factor == 3)
            rawHung.texture=hun3;
        if (factor == 2)
            rawHung.texture = hun2;
        if (factor == 1)
            rawHung.texture = hun1;
        if (factor == 0)
            rawHung.texture = hun0;
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

    float startTime = 0f;
    float holdTime = 3.0f;
    float timer = 0f;

    bool held = false;
    public void Feed()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            startTime = Time.time;
            timer = startTime;
        }

        if (Input.GetKey(KeyCode.E) && held==false){
            sharkAnim.SetBool("isFeed", true);
            feedText.SetActive(false);
            timer += Time.deltaTime;

            if (timer > (startTime+holdTime))
            {
                held = true;
                FeedDone();
            }
        }
        else
            sharkAnim.SetBool("isFeed", false);


        if (Input.GetKeyUp(KeyCode.E))
        {
            held = false;
            sharkAnim.SetBool("isFeed", false);
        }
    }
    void FeedDone()
    {
        sharkAnim.SetBool("isFeed", false);
        Debug.Log("Fed");
        aud.Play();
        energy += .25;
        sharkStat.heal(5.0);
        try
        {
            npc.Fed();
        }
        catch (Exception e)
        {
        }
        try
        {
            bnpc.Fed();
        }
        catch (Exception e)
        {
        }
        try
        {
            pnpc.Fed();
        }
        catch (Exception e)
        {
        }
        startTime = 0f;
        timer = 0f;
        held = false;
    }
    private void OnTriggerEnter(Collider spin)
    {
        if (spin.gameObject.tag == "Spin")
        {
            sharkStat.damage(pnpc.npcHp.getDam());
            Debug.Log("Shark HP: " + sharkStat.getHp());
        }

        if (spin.gameObject.tag == "SharkMouth")
        {
            sharkStat.damage(Bnpc.npcHp.getDam());
            Debug.Log("Shark HP: " + sharkStat.getHp());
        }
    }

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
