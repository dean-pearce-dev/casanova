using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/**************************************************************************************
* Type: Class
* 
* Name: UIManager
*
* Author: Charlie Taylor
*
* Description: Manage everything UI
**************************************************************************************/
public class UIManager : MonoBehaviour
{
    //What menu we are on
    private enum Menu
	{
        Main,
        Options,
        Controls,
        Credits,
        Game
	}
    //Menu we are on
    private Menu m_currentMenu = Menu.Main;

    [Header("Images")]
    [SerializeField, Tooltip("Black Screen Image")]
    private Image m_blackScreen;
    [SerializeField, Tooltip( "" )]
    Image m_background;

    [Header("Main Menu Canvas Groups")]
    [SerializeField, Tooltip( "Main Menu Canvas Group" )]
    private CanvasGroup m_mainMenu;
    [SerializeField, Tooltip( "Options Menu Canvas Group" )]
    private CanvasGroup m_optionsUIGroup;
    [SerializeField, Tooltip( "Controls Menu Canvas Group" )]
    private CanvasGroup m_controlsUIGroup;
    [SerializeField, Tooltip( "Credits Menu Canvas Group" )]
    private CanvasGroup m_creditsUIGroup;
    [SerializeField, Tooltip( "Back button used to return to main menu canvas group" )]
    private CanvasGroup m_backButton;

    [SerializeField, Tooltip( "Plot summary Canvas Group" )]
    private CanvasGroup m_plotUIGroup;

    [Header("Pause Menu Canvas Groups")]
    [SerializeField, Tooltip( "Main Pause Screen Menu" )]
    private CanvasGroup m_pauseScreen;
    [SerializeField, Tooltip( "The Pause screen back button used in options and controls when in pause" )]
    private CanvasGroup m_pauseBackButton;

    [Header("Gameplay UI")]
    [SerializeField, Tooltip( "Game UI Canvas Group" )]
    private CanvasGroup m_gameUIGroup;
    [SerializeField, Tooltip( "Canvas group for dead screen" )]
    private CanvasGroup m_deadUIGroup;
    [SerializeField, Tooltip( "Canvas group for win screen" )]
    private CanvasGroup m_winGroup;

    //When going to other screen in pause screen, we need to know what screen we are on when leaving that screen,
    //so this is populated when GOING to that screen, and then used in leaving back to menu to hide the right screen
    private string m_pauseScreenToHide;

    [Header("Fade Settings")]
    [SerializeField, Range(0.1f, 2.0f), Tooltip("How fast menu items fade in and out")]
    private float m_uiFadeInTime = 1f;

    [Header( "Fade Settings" )]
    [SerializeField, Range( 0.1f, 2.0f ), Tooltip( "How fast menu screens swap" )]
    private float m_menuSwapSpeed = 0.25f;

    [SerializeField]
    private AudioMixer m_audioMixer;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: QuitGame
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by quit game buttons. Quits the game
    **************************************************************************************/
    public void QuitGame()
	{
        Application.Quit();
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SwapMenus
    * Parameters: string menuToGoTo
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by Menu buttons. Swap menus based on string input
    **************************************************************************************/
    public void SwapMenus( string menuToGoTo )
    {
        //Temp group to store group in
        CanvasGroup groupToFadeOut;
        //Based on current menu, fill group to fade out with it's related canvasGroup
        switch ( m_currentMenu )
        {
            default: 
                groupToFadeOut = m_mainMenu; 
                break;
            case Menu.Main:
                groupToFadeOut = m_mainMenu;
                break;
            case Menu.Options:
                groupToFadeOut = m_optionsUIGroup;
                break;
            case Menu.Controls:
                groupToFadeOut = m_controlsUIGroup;
                break;
            case Menu.Credits:
                groupToFadeOut = m_creditsUIGroup;
                break;
            case Menu.Game:
                groupToFadeOut = m_gameUIGroup;
                break;
        }
        //Now fade it out
        StartCoroutine( FadeOutGroup( groupToFadeOut, m_menuSwapSpeed ) );

        //Based on target menu, fill group to fade in with it's related canvasGroup
        CanvasGroup groupToFadeIn;
        switch ( menuToGoTo )
        {
            default:
                groupToFadeIn = m_mainMenu;
                StartCoroutine( FadeOutGroup(m_backButton, m_menuSwapSpeed ) );
                m_currentMenu = Menu.Main;
                break;
            case "Options":
                //Update sliders going to menu
                GetComponent<OptionsManager>().RefreshSliders();
                groupToFadeIn = m_optionsUIGroup;
                m_currentMenu = Menu.Options;
                StartCoroutine( FadeInGroup( m_backButton, m_menuSwapSpeed, m_menuSwapSpeed ) );
                break;
            case "Controls":
                groupToFadeIn = m_controlsUIGroup;
                m_currentMenu = Menu.Controls;
                StartCoroutine( FadeInGroup( m_backButton, m_menuSwapSpeed, m_menuSwapSpeed ) );
                break;
            case "Credits":
                groupToFadeIn = m_creditsUIGroup;
                m_currentMenu = Menu.Credits;
                StartCoroutine( FadeInGroup( m_backButton, m_menuSwapSpeed, m_menuSwapSpeed ) );
                break;
        }
        //Fade Group in
        StartCoroutine( FadeInGroup( groupToFadeIn, m_menuSwapSpeed, m_menuSwapSpeed ) );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DisplayPauseMenu
    * Parameters: bool paused
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Display or hide the pause menu
    **************************************************************************************/
    public void DisplayPauseMenu(bool paused)
    {
        if ( paused )
		{
            //Pause
            //Refresh the options sliders to match the values
            GetComponent<OptionsManager>().RefreshSliders();
            m_pauseScreen.gameObject.SetActive( true );
            m_gameUIGroup.gameObject.SetActive( false );
            m_background.gameObject.SetActive( true );
            m_background.color = ChangeImageAlpha(m_background, 0.6f);
            EventManager.StartPauseGameEvent();
        }
		else
		{
            //Unpause
            m_pauseScreen.gameObject.SetActive( false );
            m_optionsUIGroup.gameObject.SetActive( false );
            m_controlsUIGroup.gameObject.SetActive( false );
            m_pauseBackButton.gameObject.SetActive( false );
            m_background.gameObject.SetActive( false );
            m_gameUIGroup.gameObject.SetActive( true );
            EventManager.StartUnpauseGameEvent();
        }

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DisplaySubPauseScreen
    * Parameters: string screen
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Display or hide the sub pause menu
    **************************************************************************************/
    public void DisplaySubPauseScreen( string screen )
	{
        switch ( screen )
		{
			case "Options":
                m_optionsUIGroup.gameObject.SetActive( true );

                break;
            case "Controls":
                m_controlsUIGroup.gameObject.SetActive( true );
                break;
		}
        m_pauseBackButton.gameObject.SetActive( true );
        m_pauseScreen.gameObject.SetActive( false );
        m_pauseScreenToHide = screen;

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReturnToPause
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Called by pause screen menus to return to main pause scree
    **************************************************************************************/
    public void ReturnToPause()
	{
        //This was populated in DisplaySubPauseScreen when this screen was displayed. Now we hide it
        switch ( m_pauseScreenToHide )
        {
            case "Options":
                m_optionsUIGroup.gameObject.SetActive( false );

                break;
            case "Controls":
                m_controlsUIGroup.gameObject.SetActive( false );
                break;
        }

        m_pauseBackButton.gameObject.SetActive( false );
        m_pauseScreen.gameObject.SetActive( true );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Respawn
    * Parameters: float howLongFade 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Fade out and back in for respawning
    **************************************************************************************/
    public void Respawn( float howLongFade )
    {
        m_pauseScreen.gameObject.SetActive( false );
        m_background.gameObject.SetActive( false );
        //Fade Black Screen in
        StartCoroutine( FadeIn( m_blackScreen, howLongFade ) );

        //Turn off You Died. Put 0.5 on it so all respawn shenanigans has time to finish while it's obscured
        StartCoroutine( FadeOutGroup( m_deadUIGroup, 0.1f, howLongFade + 0.5f ) );

        //Fade back in with 0.5 wait
        StartCoroutine( FadeOut( m_blackScreen, howLongFade, howLongFade + 0.5f ) );
        StartCoroutine( FadeInGroup( m_gameUIGroup, m_uiFadeInTime, howLongFade * 2 + 0.5f ) );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReturnToMenu
    * Parameters: float howLongFade 
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Fade screen out for returning to the menu
    **************************************************************************************/
    public void ReturnToMenu(float fadeTime)
	{

        m_pauseScreen.gameObject.SetActive( false );
        StartCoroutine( FadeIn( m_blackScreen, fadeTime ) );

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DisplayDeathUI
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Fade out game ui and fade in death ui
    **************************************************************************************/
    public void DisplayDeathUI()
	{
        StartCoroutine( FadeOutGroup( m_gameUIGroup, m_uiFadeInTime ) );
        StartCoroutine( FadeInGroup( m_deadUIGroup, m_uiFadeInTime + 0.5f ) );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: BeginScene
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Set all relevant objects to active or not, and fade from black screen in
    **************************************************************************************/
    public void BeginScene()
    {
        m_gameUIGroup.gameObject.SetActive( false );
        m_optionsUIGroup.gameObject.SetActive ( false );
        m_controlsUIGroup.gameObject.SetActive ( false );
        m_creditsUIGroup.gameObject.SetActive ( false );
        m_mainMenu.gameObject.SetActive( false );
        m_pauseScreen.gameObject.SetActive( false );
        m_pauseBackButton.gameObject.SetActive( false );
        m_deadUIGroup.gameObject.SetActive( false );
        m_plotUIGroup.gameObject.SetActive( false );

        m_blackScreen.gameObject.SetActive( true );

        StartCoroutine( FadeOut( m_blackScreen, m_uiFadeInTime ) );

        //Make Menu UI fade in delay same as Black fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_mainMenu, m_uiFadeInTime, m_uiFadeInTime / 2 ) );
        StartCoroutine( FadeIn( m_background, m_uiFadeInTime, m_uiFadeInTime / 2 ) );


    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CompleteGame
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Complete game, so fade in win screen
    **************************************************************************************/
    public void CompleteGame()
	{

        StartCoroutine( FadeOutGroup( m_gameUIGroup, m_uiFadeInTime ) );
        StartCoroutine( FadeInGroup( m_winGroup, m_uiFadeInTime ) );


    }






    public void ShowPlotUI()
	{
        StartCoroutine( FadeOutGroup( m_mainMenu, m_uiFadeInTime ) );
        StartCoroutine( FadeInGroup( m_plotUIGroup, m_uiFadeInTime, m_uiFadeInTime / 2 ) );
    }




    /**************************************************************************************
    * Type: Function
    * 
    * Name: StartGame
    * Parameters: n/a
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: Start game, so fade in gameplay UI, and also prep for stuff with pause
    **************************************************************************************/
    public void StartGame()
	{

        StartCoroutine( FadeOutGroup( m_plotUIGroup, m_uiFadeInTime ) );
        StartCoroutine( FadeOut( m_background, m_uiFadeInTime ) );
        //Make Game UI fade in delay same as menu fade out to make it look like it was queued up
        StartCoroutine( FadeInGroup( m_gameUIGroup, m_uiFadeInTime, m_uiFadeInTime / 2 ) );

        //If these screens are used in the MAIN menu, their alpha is set to 0 and deactivated.
        //To stop needing to set them to 1 eevrytime I pause, if we just do it here, they stay deactivated, but alpha is 1
        m_optionsUIGroup.alpha = 1;
        m_controlsUIGroup.alpha = 1;
        //Make Confirm Button for Options interactable
        m_optionsUIGroup.gameObject.GetComponentInChildren<Button>().interactable = true ;

        Settings.g_canPause = true;

        EventManager.StartGameBeginEvent();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ChangeImageAlpha
    * Parameters: Image image, float newAlpha
    * Return: Color
    *
    * Author: Charlie Taylor
    *
    * Description: Edit just the alpha of an image without having to make temp objects
    **************************************************************************************/
    private Color ChangeImageAlpha( Image image, float newAlpha )
	{
        return new Color( image.color.r, image.color.g, image.color.b, newAlpha);
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FadeIn
    * Parameters: Image image, float time, float delay = 0.0f 
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Fade an image in, over a set time, after a set delay
    **************************************************************************************/
    private IEnumerator FadeIn( Image image, float time, float delay = 0.0f )
    {
        //Wait
        yield return new WaitForSeconds( delay );
        //We want object to be active, obviously
        image.gameObject.SetActive( true );
        //And we are fading IN, so it needs to start at alpha 0
        image.color = ChangeImageAlpha( image, 0.0f );

        //Now we increase alpha over time
        for ( float alpha = 0.0f; alpha < 1.0f; alpha += Time.deltaTime / time )
        {
            image.color = ChangeImageAlpha( image, alpha );
            yield return null;
        }
        // Fully Set it, just to stop any weirdness
        image.color = ChangeImageAlpha( image, 1.0f );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FadeOut
    * Parameters: Image image, float time, float delay = 0.0f 
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Fade an image out, over a set time, after a set delay
    **************************************************************************************/
    public IEnumerator FadeOut( Image image, float time, float delay = 0.0f )
    {
        //Wait
        yield return new WaitForSeconds( delay );

        //And we are fading OUT, so it needs to start at alpha 1
        image.color = ChangeImageAlpha( image, 1.0f );

        //Now we decrease alpha over time
        for ( float alpha = 1.0f; alpha > 0.0f; alpha -= Time.deltaTime / time )
        {
            image.color = ChangeImageAlpha( image, alpha );
            yield return null;
        }
        // Fully Set it
        image.color = ChangeImageAlpha( image, 0.0f );

        //We want object to be disabled now as it's invisible
        image.gameObject.SetActive( false );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FadeInGroup
    * Parameters: CanvasGroup group, float time, float delay = 0.0f
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Fade a canvas group in, over a set time, after a set delay
    *              Basically Identical to FadeIn, but with tweaks just for CanvasGroups
    **************************************************************************************/
    public IEnumerator FadeInGroup( CanvasGroup group, float time, float delay = 0.0f )
    {
        //Wait
        yield return new WaitForSeconds( delay );
        //And we are fading IN, so it needs to start at alpha 0
        group.alpha = 0.0f;
        //We want object to be active, obviously
        group.gameObject.SetActive( true );

        //Over time, increase alpha
        for ( float alpha = group.alpha; alpha < 1.0f; alpha += Time.deltaTime / time )
        {
            group.alpha = alpha;
            yield return null;
        }

        //Enable Buttons
        EnableButtons( group.gameObject, true );

        group.alpha = 1.0f;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: FadeOutGroup
    * Parameters: CanvasGroup group, float time, float delay = 0.0f
    * Return: IEnumerator
    *
    * Author: Charlie Taylor
    *
    * Description: Fade a canvas group out, over a set time, after a set delay
    *              Basically Identical to FadeOut, but with tweaks just for CanvasGroups
    **************************************************************************************/
    public IEnumerator FadeOutGroup( CanvasGroup group, float time, float delay = 0.0f )
    {
        //wait
        yield return new WaitForSeconds( delay );
        
        //Disable Buttons
        EnableButtons(group.gameObject, false );
        //Start at max alpha
        group.alpha = 1.0f;

        //Fade out
        for ( float alpha = group.alpha; alpha > 0.0f; alpha -= Time.deltaTime / time )
        {
            group.alpha = alpha;
            yield return null;
        }
        //Set it to exactly 0
        group.alpha = 0.0f;

        //We want object to be disabled now as it's invisible
        group.gameObject.SetActive( false );
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: EnableButtons
    * Parameters: GameObject parent, bool enable
    * Return: n/a
    *
    * Author: Charlie Taylor
    *
    * Description: If we were to click a button when fading in or out, it could cause problems for
    *              the coroutines, so when fading a group, disable all buttons in the group
    *              so this function turns them off/on at the start/end of fade in/outs
    **************************************************************************************/
    private void EnableButtons( GameObject parent, bool enable )
    {
        Button[] activeButtons = parent.GetComponentsInChildren<Button>();

        foreach ( Button button in activeButtons )
        {
            button.interactable = enable;
        }
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: FadeInForCutscene
    * Parameters: Cutscene cutsceneScript, Image image, bool beginCutscene, float time, float delay = 0.0f
    * Return: n/a
    *
    * Author: Charlie Taylor
    *         Dean Pearce
    *
    * Description: Fade in to a black screen, then do the cutscene
    **************************************************************************************/
    public IEnumerator FadeInForCutscene( Cutscene cutsceneScript, Image image, bool beginCutscene, float time, float delay = 0.0f )
    {
        //Fade in black screen
        StartCoroutine(FadeIn( image, time, delay ));
        //Wait until that's done (So both time and delay)
        yield return new WaitForSeconds( time + delay );

        if (beginCutscene)
        {
            m_gameUIGroup.gameObject.SetActive(false);
            cutsceneScript.StartCutscene();
        }
        else
        {
            cutsceneScript.EndCutscene();
            m_gameUIGroup.gameObject.SetActive(true);
        }

        StartCoroutine(FadeOut(m_blackScreen, m_uiFadeInTime));
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CutsceneHandover
    * Parameters: Cutscene cutsceneScript
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: Begin a cutscene
    **************************************************************************************/
    public void CutsceneHandover( Cutscene cutsceneScript )
    {
        StartCoroutine( FadeInForCutscene(cutsceneScript, m_blackScreen, true, m_uiFadeInTime));
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReturnFromCutscene
    * Parameters: Cutscene cutsceneScript
    * Return: n/a
    *
    * Author: Dean Pearce
    *
    * Description: End a cutscene
    **************************************************************************************/
    public void ReturnFromCutscene( Cutscene cutsceneScript )
    {
        StartCoroutine( FadeInForCutscene(cutsceneScript, m_blackScreen, false, m_uiFadeInTime));
    }

}
