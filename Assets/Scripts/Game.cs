using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

enum GameState
{
    ADJUSTING,
    KICKED,
    SCORED,
    MISSED
}

public class Game : MonoBehaviour
{
    [SerializeField]
    public Rigidbody m_rb = null;
    [SerializeField]
    public Ball m_ball = null;
    [SerializeField]
    public GameObject m_goalie = null;
    [SerializeField]
    public Text m_statusText;
    [SerializeField]
    public Text m_scoreText;
    [SerializeField]
    public Text m_promptText;

    private GameState m_state;

    private float m_fDistanceToBall = 0f;
    private float m_fShortestDistanceToBall = 0f;
    private float m_fGoalieXPosition = 0f;

    private Vector3 m_vDefaultGoaliePosition = new Vector3(0f, 0f, 33f);

    public int m_score = 0;
    // Start is called before the first frame update
    void Start()
    {
        m_state = GameState.ADJUSTING;
        m_rb = GetComponent<Rigidbody>();
        Assert.IsNotNull(m_rb, "Missing Rigidbody component");
        Assert.IsNotNull(m_ball, "Missing Ball");
        Assert.IsNotNull(m_goalie, "Missing Goalie");
        m_fDistanceToBall = Mathf.Abs((m_ball.transform.position - transform.position).magnitude);
        m_fShortestDistanceToBall = m_fDistanceToBall;
        m_goalie.transform.position = m_vDefaultGoaliePosition;
    }

    // Update is called once per frame
    void Update()
    {
        switch (m_state)
        {
            case GameState.KICKED:
                UpdateGoalie();
                m_fDistanceToBall = Mathf.Abs((m_ball.transform.position - transform.position).magnitude);
                if (m_fDistanceToBall < m_fShortestDistanceToBall)
                    m_fShortestDistanceToBall = m_fDistanceToBall;
                if (m_fShortestDistanceToBall - m_fDistanceToBall < -5f || m_ball.GetComponent<Rigidbody>().velocity.magnitude <= 1f)
                {
                    EnterMissed();
                }
                break;
            default:
                CheckExit();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == m_ball.GetComponent<Collider>() && m_state == GameState.KICKED)
        {
            EnterScored();
        }
    }

    public void EnterAdjusting()
    {
        m_state = GameState.ADJUSTING;
        m_fDistanceToBall = Mathf.Abs((m_ball.transform.position - transform.position).magnitude);
        m_fShortestDistanceToBall = m_fDistanceToBall;
        m_statusText.text = "";
        m_promptText.text = "";
        m_goalie.transform.position = m_vDefaultGoaliePosition;
    } 

    public void EnterKicked()
    {
        m_state = GameState.KICKED;
    }

    private void EnterScored()
    {
        m_state = GameState.SCORED;
        m_score++;
        m_scoreText.text = m_score.ToString();
        m_ball.FinishKick();
        m_statusText.text = "SCORE";
        m_promptText.text = "*SPACEBAR to Reset*";
    }

    private void EnterMissed()
    {
        m_state = GameState.MISSED;
        m_ball.FinishKick();
        m_statusText.text = "MISSED";
        m_promptText.text = "*SPACEBAR to Reset*";
    }

    private void UpdateGoalie()
    {
        m_fGoalieXPosition = Mathf.Lerp(0f, m_ball.transform.position.x, 0.8f);
        m_goalie.transform.position = new Vector3(m_fGoalieXPosition, m_vDefaultGoaliePosition.y, m_vDefaultGoaliePosition.z);
    }

    private void CheckExit()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}