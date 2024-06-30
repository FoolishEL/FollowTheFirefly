using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Validators;
using Demos.Shared.Behaviours;
using UnityEngine;
using UnityEngine.UI;

namespace Demos.Simple.Behaviours
{
    public class TextBehaviour : MonoBehaviour
    {
        private Text text;
        private AgentBehaviour agent;
        private HungerBehaviour hunger;

        private void Awake()
        {
            text = GetComponentInChildren<Text>();
            agent = GetComponent<AgentBehaviour>();
            hunger = GetComponent<HungerBehaviour>();
        }

        private void Update()
        {
            text.text = GetText();
        }

        private string GetText()
        {
            if (agent.CurrentAction is null)
                return "Idle";

            return $"{agent.CurrentGoal.GetType().GetGenericTypeName()}\n{agent.CurrentAction.GetType().GetGenericTypeName()}\n{agent.State}\nhunger: {hunger.hunger:0.00}";
        }
    }
}