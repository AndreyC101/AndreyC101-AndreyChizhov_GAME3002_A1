using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum BallState
{
    SETUP,
    LAUNCHED,
    DONE
}

public class Ball : MonoBehaviour
{
    [SerializeField]
    public Game m_game;
    [SerializeField]
    public Camera m_mainCamera;
    [SerializeField]
    public Camera m_playCamera;

    private Vector3 m_vDefaultPosition = new Vector3(0f, 0.15f, 0f);
    private Vector3 m_vDefaultTargetPosition = new Vector3(0f, 1f, 25f);
    private Vector3 m_vHiddenTargetPosition = new Vector3(0f, -10f, 0f);
    
    private Vector3 m_vTargetPosition;
    private Vector3 m_vInitialVelocity;

    private Rigidbody m_rb = null;
    private GameObject m_targetDisplay = null;

    [SerializeField]
    public float m_fRotationFactor = 0.03f;
    [SerializeField]
    public float m_fTargetHeightFactor = 0.005f;

    private float m_fRotationTheta = 0f;
    private float m_fTargetHeight = 0f;
    private float m_fDistanceToTarget = 0f;

    public BallState m_state;

    // Start is called before the first frame update
    void Start()
    {
        m_mainCamera.enabled = true;
        m_playCamera.enabled = false;
        m_state = BallState.SETUP;
        m_rb = GetComponent<Rigidbody>();
        Assert.IsNotNull(m_rb, "Missing Rigidbody component");
        Assert.IsNotNull(m_game, "Missing Game");
        Assert.IsNotNull(m_mainCamera, "Missing Main Camera");
        Assert.IsNotNull(m_playCamera, "Missing Play Camera");
        m_vTargetPosition = m_vDefaultTargetPosition;
        m_fTargetHeight = m_vTargetPosition.y;
        CreateTargetDisplay();
        m_fDistanceToTarget = (m_targetDisplay.transform.position - transform.position).magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        switch (m_state)
        {
            case BallState.SETUP:
                m_mainCamera.transform.LookAt(m_targetDisplay.transform);
                UpdateTargetDisplay();
                HandleUserInput();
                break;
            case BallState.LAUNCHED:
                m_playCamera.transform.LookAt(transform);
                break;
            case BallState.DONE:
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    Reset();
                }
                break;
        }
    }

    private void CreateTargetDisplay()
    {
        m_targetDisplay = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_targetDisplay.transform.position = m_vTargetPosition;
        m_targetDisplay.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Color targetColor = Color.red;
        targetColor.a = 0.3f;
        m_targetDisplay.GetComponent<Renderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
        m_targetDisplay.GetComponent<Renderer>().material.color = targetColor;
        m_targetDisplay.GetComponent<Collider>().enabled = false;
    }

    private void UpdateTargetDisplay()
    {
        Vector3 targetPosition = transform.forward * m_fDistanceToTarget;
        targetPosition = new Vector3(targetPosition.x, m_fTargetHeight, targetPosition.z);
        m_targetDisplay.transform.position = targetPosition;
    }

    void RotateHorizontal(float theta)
    {
        m_fRotationTheta += theta;
        transform.rotation = Quaternion.Euler(0f, m_fRotationTheta, 0f);
    }

    void AdjustTargetHeight(float displacement)
    {
        m_fTargetHeight = Mathf.Clamp(m_targetDisplay.transform.position.y + displacement, 1f, 10f);
        m_targetDisplay.transform.position = new Vector3(m_targetDisplay.transform.position.x,
                m_fTargetHeight,
                m_targetDisplay.transform.position.z);
    }

    public void SwapCameras()
    {
        m_mainCamera.enabled = !m_mainCamera.enabled;
        m_playCamera.enabled = !m_playCamera.enabled;
    }

    private void OnKickBall()
    {
        // H = Vi^2 * sin^2(theta) / 2g
        // R = 2Vi^2 * cos(theta) * sin(theta) / g

        //Vi = sqrt(2gh) / sin(tan^-1(4h/r))
        //theta = tan^-1(4h/r)

        //Vy = V * sin(theta)
        //Vx = V * cos(theta) * cos(rotationTheta)
        //Vz = V * cos(theta) * sin(rotationTheta)
        m_state = BallState.LAUNCHED;

        m_fDistanceToTarget = (m_targetDisplay.transform.position - transform.position).magnitude;

        float fMaxHeight = m_targetDisplay.transform.position.y;
        float fRange = (m_fDistanceToTarget * 2);
        float fTheta = Mathf.Atan((4 * fMaxHeight) / (fRange));

        float fInitVelMag = Mathf.Sqrt((2 * Mathf.Abs(Physics.gravity.y) * fMaxHeight)) / Mathf.Sin(fTheta);

        m_vInitialVelocity.y = fInitVelMag * Mathf.Sin(fTheta);
        m_vInitialVelocity.z = fInitVelMag * Mathf.Cos(fTheta) * Mathf.Cos(m_fRotationTheta * Mathf.Deg2Rad);
        m_vInitialVelocity.x = fInitVelMag * Mathf.Cos(fTheta) * Mathf.Sin(m_fRotationTheta * Mathf.Deg2Rad);

        m_rb.velocity = m_vInitialVelocity;

        m_targetDisplay.transform.position = m_vHiddenTargetPosition; //hide the target
        SwapCameras();
        m_game.EnterKicked();
    }

    public void FinishKick()
    {
        m_state = BallState.DONE;
    }

    public void Reset()
    {
        SwapCameras();
        transform.position = m_vDefaultPosition; 
        m_vTargetPosition = m_vDefaultTargetPosition;
        m_fTargetHeight = m_vTargetPosition.y;
        m_fRotationTheta = 0f;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        m_rb.velocity = Vector3.zero;
        m_rb.angularVelocity = Vector3.zero;
        UpdateTargetDisplay();
        m_state = BallState.SETUP;
        m_game.EnterAdjusting();
    }

    private void HandleUserInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            AdjustTargetHeight(m_fTargetHeightFactor);
        }
        if (Input.GetKey(KeyCode.S))
        {
            AdjustTargetHeight(-m_fTargetHeightFactor);
        }
        if (Input.GetKey(KeyCode.A))
        {
            RotateHorizontal(-m_fRotationFactor);
        }
        if (Input.GetKey(KeyCode.D))
        {
            RotateHorizontal(m_fRotationFactor);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnKickBall();
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
