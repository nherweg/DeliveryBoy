﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginExperiment : MonoBehaviour
{
    public UnityEngine.GameObject greyedOutButton;
    public UnityEngine.GameObject beginExperimentButton;
    public UnityEngine.UI.InputField participantCodeInput;
    public UnityEngine.UI.Toggle useRamulatorToggle;
    public UnityEngine.UI.Text beginButtonText;

    private const string scene_name = "MainGame";

    private void Update()
    {
        if (IsValidParticipantName(participantCodeInput.text))
        {
            beginExperimentButton.SetActive(true);
            greyedOutButton.SetActive(false);
            beginButtonText.text = "Begin session " + NextSessionNumber().ToString();
        }
        else
        {
            greyedOutButton.SetActive(true);
            beginExperimentButton.SetActive(false);
        }
    }

    public void DoBeginExperiment()
    {
        DeliveryExperiment.ConfigureExperiment(useRamulatorToggle.isOn, 1);
        SceneManager.LoadScene(scene_name);
    }

    private int NextSessionNumber()
    {
        string dataPath = UnityEPL.GetDataPath();
        string[] sessionFolders = System.IO.Directory.GetDirectories(dataPath);
        int mostRecentSessionNumber = -1;
        foreach (string folder in sessionFolders)
        {
            int thisSessionNumber = -1;
            if (int.TryParse(folder, out thisSessionNumber) && thisSessionNumber > mostRecentSessionNumber)
                mostRecentSessionNumber = thisSessionNumber;
        }
        return mostRecentSessionNumber + 1;
    }

    private bool IsValidParticipantName(string name)
    {
        bool isTest = name.Equals("TEST");
        if (isTest)
            return true;
        if (name.Length != 6)
            return false;
        bool isValidRAMName = name[0].Equals('R') && name[1].Equals('1') && char.IsDigit(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]) && char.IsUpper(name[5]);
        bool isValidSCALPName = char.IsUpper(name[0]) && char.IsUpper(name[1]) && char.IsUpper(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]) && char.IsDigit(name[5]);
        return isValidRAMName || isValidSCALPName;
    }
}