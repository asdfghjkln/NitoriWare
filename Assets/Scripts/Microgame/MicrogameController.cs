﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
//using UnityEngine.SceneManagement;

public class MicrogameController : MonoBehaviour
{
	public static MicrogameController instance;


	public ControlScheme controlScheme;
	public int beatDuration;
	public string command;
	public bool defaultVictory, canEndEarly;
	public AudioClip musicClip;

	public bool debugMusic, debugCommand, debugTimer, debugTimerTick;
	[Range(1, ScenarioController.MAX_SPEED)]
	public int debugSpeed;
	public GameObject debugObjects;

	private bool victory, victoryDetermined;

	public enum ControlScheme
	{
		Touhou,
		Mouse
	}

	void Awake()
	{
		instance = this;

		if (ScenarioController.instance == null)
		{
			//Debug Mode Start (scene open by itself)
			ScenarioController.beatLength = 60f / 130f;
			Time.timeScale = ScenarioController.getSpeedMult(debugSpeed);

			debugObjects = Instantiate(debugObjects, Vector3.zero, Quaternion.identity) as GameObject;

			MicrogameTimer.instance = debugObjects.transform.FindChild("UI Camera").FindChild("Timer").GetComponent<MicrogameTimer>();
			MicrogameTimer.instance.beatsLeft = (float)beatDuration;
			if (!debugTimer)
				MicrogameTimer.instance.disableDisplay = true;
			if (debugTimerTick)
				MicrogameTimer.instance.invokeTick();

			victory = defaultVictory;
			victoryDetermined = false;

			if (debugMusic && musicClip != null)
			{
				AudioSource source = debugObjects.transform.FindChild("Music").GetComponent<AudioSource>();
				source.clip = musicClip;
				source.pitch = ScenarioController.getSpeedMult(debugSpeed);
				source.Play();
			}

			Transform UICam = debugObjects.transform.FindChild("UI Camera");
			UICam.gameObject.SetActive(true);
			if (debugCommand)
			{
				UICam.FindChild("Command").gameObject.SetActive(true);
				UICam.FindChild("Command").FindChild("Text").GetComponent<TextMesh>().text = command;
			}

			if (controlScheme == ControlScheme.Mouse)
				Cursor.visible = true;
		}
		else
		{
			//Normal Awaken
			//Gives info about game to Scenario, then objects get deactivated in Start() until the game is ready to be loaded proper
			ScenarioController.instance.updateMicrogameInfo();

			//TODO add OnAwake event
			//For when a microgame needs to change its command via code

		}

	}

	void Start()
	{
		if (ScenarioController.instance != null)
		{
			//Awaken after we
			ScenarioController.instance.updateMicrogameInfo();
			setMicrogameActive(false);	
		}
	}

	public void startMicrogame()
	{
		//Begin the microgame proper
		setMicrogameActive(true);

		ScenarioController.instance.scenarioCamera.tag = "Camera";
		SceneManager.SetActiveScene(gameObject.scene);

		Camera.main.GetComponent<AudioListener>().enabled = false;

		MicrogameTimer.instance.beatsLeft = ScenarioController.instance.getBeatsRemaining();
		MicrogameTimer.instance.gameObject.SetActive(true);

		ScenarioController.instance.resetVictory();
		ScenarioController.instance.invokeNextCycle();
	}

	public void setMicrogameActive(bool active)
	{
		//TODO FIX THIS
		GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
		for (int i = 0; i < gameObjects.Length; i++)
		{
			gameObjects[i].SetActive(active);
		}
	}


	/// <summary>
	/// Call this to have the player win/lose a microgame, set final to true if the victory status will NOT be changed again
	/// </summary>
	/// <param name="victory"></param>
	/// <param name="final"></param>
	public void setVictory(bool victory, bool final)
	{
		if (ScenarioController.instance == null)
		{
			if (victoryDetermined)
			{
				return;
			}
			this.victory = victory;
			victoryDetermined = final;
		}
		else
		{
			ScenarioController.instance.setMicrogameVictory(victory, final);
		}
	}
	
	/// <summary>
	/// Returns whether the game would be won if it ends now
	/// </summary>
	/// <returns></returns>
	public bool getVictory()
	{
		if (ScenarioController.instance == null)
		{
			return victory;
		}
		else
			return ScenarioController.instance.getMicrogameVictory();
	}

	/// <summary>
	/// Returns true if the game's victory outcome will not be changed for the rest of its duration
	/// </summary>
	/// <returns></returns>
	public bool getVictoryDetermined()
	{
		if (ScenarioController.instance == null)
		{
			return victoryDetermined;
		}
		else
			return ScenarioController.instance.getVictoryDetermined();
	}

	void Update ()
	{
		if (ScenarioController.instance == null && Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene(gameObject.scene.buildIndex);
		}
	}
}
