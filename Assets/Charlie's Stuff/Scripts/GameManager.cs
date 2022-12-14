using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


// Names of rooms of the game. Used for Respawn Points and triggering Enemies
//THE ORDER OF THESE CORRELATE WITH THE ARRAY FOR THE RESPAWN POINT TRANSFORMS IN RespawnManager.cs
// !!!!! DO NOT CHANGE WITHOUT TAKING THIS INTO ACCOUNT !!!!!
public enum Room
{
    Cell,
    Hall,
    Armory1,
    Armory2,
    GuardRoom,
    Arena
}

/**************************************************************************************
* Type: Class
* 
* Name: Game Manager
*
* Author: Charlie Taylor
*
* Description: Main Game manager that does A LOT of the heavy lifting when it comes to 
*              global events such as respawning and enemy spawning and UI
**************************************************************************************/
public class GameManager : MonoBehaviour
{
    [SerializeField, Tooltip("Camera Manager Animator, to manage the Menu > Gameplay camera")]
    private Animator m_cinemachineAnimator;

    [Header( "Player" )]
    [SerializeField, Tooltip("The Player's Controller script")]
    private PlayerController m_playerController;
    [SerializeField, Tooltip( "The Player's Damage Manager script" )]
    private PlayerDamageManager m_playerHealthManager;

    //The room the player is currently in (Based on room completion, not physical location)
    private Room m_currentRoom;

    //Managers on the same Game Object, populated in start
    private UIManager m_uiManager;
    private RespawnManager m_respawnManager;
    private GateManager m_gateManager; 
    private AIManager m_aiManager;
    private SpawnManager m_spawnManager;

    //A bool that is updated based on if the room you are in is complete.
    private bool m_roomComplete;

    [Header("Cell Exit Trigger")]
    [SerializeField, Tooltip( "The trigger box outside of the Player's starting Cell" )]
    private GameObject m_cellExitTrigger;

    [Header( "Pause" )]
    [SerializeField, Tooltip( "Pause Input control" )]
    private InputActionReference m_pauseInput;

    [Header("Respawn")]
    [SerializeField, Range(0.5f, 4.0f), Tooltip( "How long for screen to fade out when respawning")]
    private float m_respawnfadeTime = 4.0f;
    //Scene load, for reseting game
    AsyncOperation m_sceneLoad;

    [Header("Cursor Stuff")]
    [SerializeField]
    private GameObject m_cursorSprite;


    [SerializeField, Tooltip( "THE SWORD TABLE INTERACTION PREFAB" )]
    private GameObject m_swordTableInteractionPrefab;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OnEnable
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Enable input for pause (No OnDisable as this object is never disabled)
    **************************************************************************************/
    private void OnEnable()
	{
        m_pauseInput.action.Enable();

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Populate member variables and some global variables
    **************************************************************************************/
    private void Start()
    {
        Settings.g_inMenu = true;
        //Very Begining of Game
        Settings.g_canPause = false;
        Settings.g_paused = false;
        //Components attached to same GO
        m_uiManager = GetComponent<UIManager>();
        m_respawnManager = GetComponent<RespawnManager>();
        m_gateManager = GetComponent<GateManager>();
        m_aiManager = GetComponent<AIManager>();
        m_spawnManager = GetComponent<SpawnManager>();

        //Always begin in cell
        m_currentRoom = Room.Cell;
        m_respawnManager.SetRespawnPoint( m_currentRoom );

        //Call UI Managers begin scene script to begin showing menu
        m_uiManager.BeginScene();

        ResetRoomEnemies( Room.Cell );


    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check Pause and Room Completion
    **************************************************************************************/
    private void Update()
	{
        //Check if Paused clicked, and manage it
        PauseCheck();
        //Room Completion check
        RoomCompleteCheck();

    }


    public void ShowPlot()
	{
        m_uiManager.ShowPlotUI();
	}


    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayGame
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Play Game button in Main Menu, this does all the relevant stuff
    *              to begin the game
    **************************************************************************************/
    public void PlayGame()
    {
        //Manage UI Elements
        m_uiManager.StartGame();
        //Unlock player menu lock  
        m_playerController.SetMenuLock( false );
        //Name of Cinemachine animator's game state. Not a variable as this is the ONLY place it is used, which makes it equal to a variable in time written
        m_cinemachineAnimator.Play( "Game State" );
        //Wake up player via animation. Not a variable as this is the ONLY place it is used, which makes it equal to a variable in time written
        m_playerController.gameObject.GetComponent<Animator>().SetTrigger( "WakeUp" );
        //Melee controller is disabled by default as clicking play will queue up attacks and it's just easier this way
        m_playerController.GetComponent<MeleeController>().enabled = true;

        //Hide cursor

        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;

        EnterAMenu( false );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PauseCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if Pause was pressed, and if so, pause or unpause
    **************************************************************************************/
    private void PauseCheck()
	{
        //Pressed
        if ( m_pauseInput.action.triggered )
        {
            //If we CAN pause, so, not at end of game or menu or something
            if ( Settings.g_canPause )
            {

                //If we are ALREADY paused, unpause
                if ( Settings.g_paused )
                {
                    //Time scale isn't the BEST but it works
                    Time.timeScale = 1;
                    Settings.g_paused = false;

                    EnterAMenu( false );
                }
                else //Pause
                {
                    Time.timeScale = 0;
                    Settings.g_paused = true;
                    //Show cursor on pause screen
                    EnterAMenu( true );
                }
                //Hide or display pause UI based on if we jsut paused or not
                m_uiManager.DisplayPauseMenu( Settings.g_paused );
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoomCompleteCheck
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Check if what rooms are completed, and open gates and stuff based on that
    *              All of this is GROSS but it was the best I could do for the time, skill
    *              and it works for the scale of the game
    **************************************************************************************/
    private void RoomCompleteCheck()
	{
        //If room is not complete...
        if ( !m_roomComplete )
        {
            //Check CURRENT room...
            switch ( m_currentRoom )
            {
                //And based on that room...
                case Room.Cell:

                    break;

                case Room.Hall:
                    //Check if this specific group is all dead
                    if ( m_aiManager.RemainingEnemiesInGroup( 0 ) <= 0 )
                    {
                        //If so, complete the room (Case room)
                        CompleteRoom( m_currentRoom );
                    }
                    break;
                case Room.Armory1:
                    if ( m_aiManager.RemainingEnemiesInGroup( 1 ) <= 0 )
                    {
                        CompleteRoom( m_currentRoom );
                        //Make Sword Grabable
                        m_swordTableInteractionPrefab.SetActive(true);
                    }
					break;
				case Room.Armory2:
                    if ( m_aiManager.RemainingEnemiesInGroup( 2 ) <= 0 )
                    {
                        CompleteRoom( m_currentRoom );
                    }
                    break;
				case Room.GuardRoom:
                    if ( m_aiManager.RemainingEnemiesInGroup( 3 ) <= 0 )
                    {
                        CompleteRoom( m_currentRoom );
                    }
                    break;
                case Room.Arena:

                    if ( m_aiManager.RemainingWaveEnemies() <= 0 )
                    {
                        if (m_spawnManager.GetCurrentWave() < m_spawnManager.GetTotalWaves())
                        {
                            EventManager.StartSpawnWaveEvent();
                        }
                        else
                        {
                            CompleteRoom( m_currentRoom );
                        }
                    }
                    break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReturnToMenu
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called in pause, when you die, or complete the game to return to the main    
    *              menu. It simply just reloads the scene.
    **************************************************************************************/
    public void ReturnToMenu()
    {
        //Make player not able to move and invulnerable to prevent possible death, which creates it's own coroutines which may cause issues
        m_playerController.LoseControl();
        m_playerHealthManager.SetInvulnerable( true );
        //Make sure timescale is returned to 1 before the coroutine
        Time.timeScale = 1f;
        StartCoroutine(ReloadScene());
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReloadScene
    * Parameters: n/a
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Fade out game, wait an additional 0.1 seconds so as to make sure screen
    *              is fully black before starting load so as to avoid weird effects
    **************************************************************************************/
    private IEnumerator ReloadScene()
	{
        float respawnFadeTime = 2.0f;
        m_uiManager.ReturnToMenu( respawnFadeTime );
        yield return new WaitForSeconds( respawnFadeTime + 0.1f );
        m_sceneLoad = SceneManager.LoadSceneAsync( SceneManager.GetActiveScene().name );
        while ( !m_sceneLoad.isDone )
        {
            yield return null;
        }
        //Set static bool for picked up sword back to false
        Settings.g_pickedUpSword = false;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Die
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: When Player dies, fade in UI to allow respawn
    **************************************************************************************/
    public void Die()
	{
        Settings.g_canPause = false;

        EnterAMenu( true );
        m_uiManager.DisplayDeathUI();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RespawnFromMenu
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Function on the Pause and Death menu to respawn. (Step 1a for respawning)
    **************************************************************************************/
    public void RespawnFromMenu()
	{
        Time.timeScale = 1;

        /* This is done to prevent the player doing a melee 
         * attack before pausing, clicking respawn and then 
         * when unpaused, an animation event regains their control */
        m_playerController.SetMenuLock( true );
        m_playerController.LoseControl();
        //Invulnerable to prevent death to prevent weirdness
        m_playerHealthManager.SetInvulnerable( true );
        //BEGIN respawning
        BeginRespawn();
        EnterAMenu( true );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: BeginRespawn
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by RespawnFromMenu, begin fading out screen in UI, then call respawn
    *              manager's respawn function. (Step 2 of Respawn)
    **************************************************************************************/
    private void BeginRespawn()
    {
        //Both these need same value. Used to fade out for that time, and delay the ACTUAL stuff until that's done
        m_uiManager.Respawn( m_respawnfadeTime );
        //UI Manager puts an additional 0.5seconds on the clock so it's fully black when do everything in respawn Manager
        //This is part 3 of respawn
        StartCoroutine( m_respawnManager.Respawn( m_respawnfadeTime ) );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ActuallyRespawn
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: ACTUALLY do respawn stuff. Basically reset everything to make it seem
    *              like it was when you first progressed to this point
    *              (Part 4 of Respawn)
    **************************************************************************************/
    public void ActuallyRespawn()
    {
        //This deactivates all enemies and returns them to the spawn pool
        m_aiManager.DeactivateActiveEnemies();
        //If it was paused, 100% not anymore
        Settings.g_paused = false;
        //Unlock player from RegainControl lock from menus
        m_playerController.SetMenuLock( false );
        //Set a room for the respawn location
        Room respawnPoint = m_respawnManager.GetRespawnPoint();
        //Based on room, load enemies correctly
        ResetRoomEnemies( respawnPoint );
        //Just reset the gate for relative room
        m_gateManager.ResetGate( respawnPoint );

        Settings.g_inMenu = false;
        Settings.g_canPause = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ResetRoomEnemies
    * Parameters: Room room 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Based on room you are respawning into, reset enemy groups
    *              I know it is disgusting and not good. Oh Well
    **************************************************************************************/
    private void ResetRoomEnemies( Room room )
	{
        switch ( room )
        {
            case Room.Cell:
                //Spawn Hall Enemies
                EventManager.StartSpawnEnemiesEvent( 0 );
                //Spawn Grunts
                EventManager.StartSpawnEnemiesEvent( 1 );
                //AND guards
                EventManager.StartSpawnEnemiesEvent( 2 );

                //Make sure trigger is active
                m_cellExitTrigger.SetActive( true );
                break;
            case Room.Hall:
                //We don't need to respawn hall enemies
                //But we do need Armory's grunts and guards
                EventManager.StartSpawnEnemiesEvent( 1 );
                EventManager.StartSpawnEnemiesEvent( 2 );
                //Spawn enemies in Prep for the NEXT NEXT room
                break;
            case Room.Armory1:
                //You have defeated the grunts, but not picked up the sword
                //So we wanna spawn the guards again, but not alert them or 
                //spawn group 3 as sword will do that
                EventManager.StartSpawnEnemiesEvent( 2 );

                if ( Settings.g_pickedUpSword )
                {
                    //If we do have the sword already
                    EventManager.StartAlertEnemiesEvent( 2 );
                    //We now spawn the next room too
                    EventManager.StartSpawnEnemiesEvent( 3 );
                }




                
                break;
            case Room.Armory2 :
                EventManager.StartSpawnEnemiesEvent( 3 );
                //Spawn enemies in Prep for the NEXT NEXT room
                EventManager.StartSpawnEnemiesEvent( 4 );
                break;
            case Room.GuardRoom:
                //Seems redundant, but if you CHOOSE to respawn after defeating the guard room, respawn in there
                //But once you go through the gate, you're now in there

                EventManager.StartSpawnEnemiesEvent( 4 );
                break;
            case Room.Arena:
                //Reprepping the arena. Restart all waves. Would be nice to save the wave you were on but.. no
                EventManager.StartSpawnEnemiesEvent( 4 );
                EventManager.StartAlertEnemiesEvent( 4 );
                EventManager.StartSpawnWaveEvent();
                break;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EnterRoom
    * Parameters: Room room 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Enter a room, updating the current room, and setting it to incomplete
    *              immediately
    **************************************************************************************/
    public void EnterRoom( Room room )
	{
        m_roomComplete = false;
        m_currentRoom = room;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CompleteRoom
    * Parameters: Room room 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: When all enemies are defeated, and you want the gate to open, call this
    **************************************************************************************/
    public void CompleteRoom( Room room )
    {
        //Room complete (Basically to stop checking if they're all dead)
        m_roomComplete = true;
        switch( room )
        {
            case Room.Cell:
                //Never anything in here
                break;
            case Room.Hall:
                //Complete the cell hallway, so open exit and reset respawn
                m_gateManager.OpenCellHallExitGate();
                m_respawnManager.SetRespawnPoint( Room.Hall );
                break;
            case Room.Armory1:
                m_respawnManager.SetRespawnPoint( Room.Armory1 );
                m_currentRoom = Room.Armory2;
                break;
            case Room.Armory2:
                m_gateManager.OpenArmoryExitGate();
                m_respawnManager.SetRespawnPoint( Room.Armory2 );
                break;
            case Room.GuardRoom:
                m_gateManager.OpenGuardRoomExitGate();
                m_respawnManager.SetRespawnPoint( Room.GuardRoom );
                break;
            case Room.Arena:
                //Entering Arena sets Arena respawn point
                //no new respawn point, you can't die now, and if you somehow did, or respawn via menu, re do the arena
                m_gateManager.OpenArenaExitGate();
                break;
        }
	}


    private void EnterAMenu(bool inMenu)
	{
        Settings.g_inMenu = inMenu;

        if ( inMenu )
		{

            if (Settings.g_currentControlScheme == Settings.g_gamepadScheme )
		    {
                m_cursorSprite.SetActive( true );
                Cursor.visible = false;
            }
		    else
            {
                m_cursorSprite.SetActive( false );
                Cursor.visible = true;
            }
        }
		else
		{
            m_cursorSprite.SetActive( false );
            Cursor.visible = false;
            
        }
    }


}
