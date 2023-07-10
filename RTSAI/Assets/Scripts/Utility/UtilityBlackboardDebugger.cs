using System;
using System.Collections.Generic;
using RTS.AI.Tools;
using TMPro;
using UnityEngine;

namespace RTS.AI.Debugger
{
    public class UtilityBlackboardDebugger : UIDebugger
    {
        [SerializeField] private UtilityBlackboard _blackboard;
        [SerializeField] private GameObject _scrollContent;
        [SerializeField] private GameObject _valuePrefab;
        
        private TMP_Text[] _scrollTextsInfo;
        private GaugeFiller[] _scrollGauges;
        private List<UtilityBlackboardValue> _orderedBlackboardValues;

        private void Awake()
        {
            _orderedBlackboardValues = new List<UtilityBlackboardValue>(_blackboard.Values.ToArray());
            _orderedBlackboardValues.Sort((lhs, rhs) =>
                String.Compare(lhs.Name, rhs.Name, StringComparison.Ordinal));

            _scrollTextsInfo = new TMP_Text[_orderedBlackboardValues.Count];
            _scrollGauges = new GaugeFiller[_orderedBlackboardValues.Count];

            for (int textIndex = 0; textIndex < _orderedBlackboardValues.Count; textIndex++)
            {
                GameObject valuePrefab = Instantiate(_valuePrefab, _scrollContent.transform);

                _scrollTextsInfo[textIndex] = valuePrefab.GetComponentInChildren<TMP_Text>();
                _scrollGauges[textIndex] = valuePrefab.GetComponentInChildren<GaugeFiller>();
            }
        }

        public override void UpdateUI()
        {
            for (int textIndex = 0; textIndex < _orderedBlackboardValues.Count; textIndex++)
            {
                UtilityBlackboardValue valueInfo = _orderedBlackboardValues[textIndex];

                string richText = "<color=black>" + valueInfo.Name + "</color>";
                
                _scrollTextsInfo[textIndex].text = richText;
                _scrollGauges[textIndex].SetGaugeValue(valueInfo.Value);
            }
        }
    }   
}