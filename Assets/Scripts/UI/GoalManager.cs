using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public struct Goal
{
    public GoalManager.OnboardingGoals CurrentGoal;
    public bool Completed;

    public Goal(GoalManager.OnboardingGoals goal)
    {
        CurrentGoal = goal;
        Completed = false;
    }
}

public class GoalManager : MonoBehaviour
{
    public enum OnboardingGoals
    {
        Empty,
        FindSurfaces,
        TapSurface
    }

    [Serializable]
    public class Step
    {
        [SerializeField] public GameObject stepObject;
        [SerializeField] public string buttonText;
        [SerializeField] public bool includeSkipButton;
    }

    [SerializeField] List<Step> m_StepList = new List<Step>();
    public List<Step> stepList { get => m_StepList; set => m_StepList = value; }

    // Bỏ ObjectSpawner khỏi đây vì không cần thiết
    // [SerializeField] ObjectSpawner m_ObjectSpawner;
    // public ObjectSpawner objectSpawner { get => m_ObjectSpawner; set => m_ObjectSpawner = value; }

    [SerializeField] GameObject m_GreetingPrompt;
    public GameObject greetingPrompt { get => m_GreetingPrompt; set => m_GreetingPrompt = value; }

    [SerializeField] GameObject m_OptionsButton;
    public GameObject optionsButton { get => m_OptionsButton; set => m_OptionsButton = value; }

    const int k_NumberOfSurfacesTappedToCompleteGoal = 1;

    Queue<Goal> m_OnboardingGoals;
    Coroutine m_CurrentCoroutine;
    Goal m_CurrentGoal;
    bool m_AllGoalsFinished;
    int m_SurfacesTapped;
    int m_CurrentGoalIndex = 0;

    void Update()
    {
        if (Pointer.current != null &&
            Pointer.current.press.wasPressedThisFrame &&
            !m_AllGoalsFinished &&
            m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces)
        {
            if (m_CurrentCoroutine != null)
            {
                StopCoroutine(m_CurrentCoroutine);
            }
            CompleteGoal();
        }
    }

    void CompleteGoal()
    {
        // Loại bỏ đăng ký sự kiện từ ObjectSpawner vì không còn sử dụng
        // if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
        //     m_ObjectSpawner.objectSpawned -= OnObjectSpawned;

        m_CurrentGoal.Completed = true;
        m_CurrentGoalIndex++;
        if (m_OnboardingGoals.Count > 0)
        {
            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            m_StepList[m_CurrentGoalIndex].stepObject.SetActive(true);
        }
        else
        {
            m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            m_AllGoalsFinished = true;
            return;
        }
    }

    // Loại bỏ OnObjectSpawned vì không cần thiết
    // void OnObjectSpawned(GameObject spawnedObject)
    // {
    //     m_SurfacesTapped++;
    //     if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface && m_SurfacesTapped >= k_NumberOfSurfacesTappedToCompleteGoal)
    //     {
    //         CompleteGoal();
    //     }
    // }

    public void StartCoaching()
    {
        Debug.Log("StartCoaching() called");
        if (m_OnboardingGoals != null)
        {
            m_OnboardingGoals.Clear();
        }

        m_OnboardingGoals = new Queue<Goal>();
        if (!m_AllGoalsFinished)
        {
            var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
            m_OnboardingGoals.Enqueue(findSurfaceGoal);
        }

        var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);
        m_OnboardingGoals.Enqueue(tapSurfaceGoal);

        m_CurrentGoal = m_OnboardingGoals.Dequeue();
        m_AllGoalsFinished = false;
        m_CurrentGoalIndex = 0;

        m_GreetingPrompt.SetActive(false);
        m_OptionsButton.SetActive(true);

        for (int i = 0; i < m_StepList.Count; i++)
        {
            if (i == 0)
            {
                m_StepList[i].stepObject.SetActive(true);
            }
            else
            {
                m_StepList[i].stepObject.SetActive(false);
            }
        }
    }
}
