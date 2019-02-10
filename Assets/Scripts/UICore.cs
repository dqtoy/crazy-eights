using TMPro;
using DG.Tweening;
using UnityEngine;

public class UICore : MonoBehaviour {

    public GameObject mainMenuHUD;                          // Main menu HUD
    public GameObject rulesOverlay;                         // Rules HUD
    public GameObject deckPlaceholder;                      // Deck image
    public GameObject gameEndOverlay;                       // Game over HUD
    public GameObject ingameOverlay;                        // Ingame HUD
    public GameObject turnArrow;

    private TextMeshProUGUI winnerTxt;                      // Winner text
    private TextMeshProUGUI drawCardPromptTxt;              // Draw card prompt

    private Vector3 winnerTxtPos;
    private Vector3[] avatarPos = new Vector3[2];

    private DOTweenAnimation turnArrowAnim;

    // Singleton code
    private static UICore _instance;
    public static UICore Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        // Make sure this is a singleton
        if (_instance == null)
        {
            _instance = this;
        }

        DontDestroyOnLoad(this);

        // Hide the rules panel
        rulesOverlay.SetActive(false);

        // Get the needed references
        winnerTxt = gameEndOverlay.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        drawCardPromptTxt = ingameOverlay.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        turnArrowAnim = turnArrow.GetComponent<DOTweenAnimation>();

        winnerTxtPos = winnerTxt.transform.localPosition;

        // Get positions of the avatar heads
        avatarPos[0] = ingameOverlay.transform.GetChild(0).localPosition;
        avatarPos[1] = ingameOverlay.transform.GetChild(1).localPosition;
    }

    public void OnNewTurn(int playerTurn)
    {
        // Pause the animation on the arrow
        turnArrowAnim.DOPause();

        // Show the arrow over the player's avatar
        turnArrow.SetActive(true);

        Vector3 newPos = avatarPos[playerTurn];
        if (playerTurn == 0)
        {
            newPos.y += 250;
            turnArrow.transform.localEulerAngles = new Vector3(0, 0, -90);
        }
        else
        {
            newPos.y -= 250;
            turnArrow.transform.localEulerAngles = new Vector3(0, 0, 90);
        }
        turnArrow.transform.localPosition = newPos;

        // Restart the animation on the arrow ( "true" means the new position will be taken into account )
        turnArrowAnim.DORestart(true);
    }

    /////////////////////////////////////////////////////////////////////////////////////////

    // ( SHOW / HIDE ) METHODS for different parts of the HUD

    /////////////////////////////////////////////////////////////////////////////////////////

    public void ShowRulesOverlay()
    {
        // TODO: animate these transitions
        mainMenuHUD.SetActive(false);
        rulesOverlay.SetActive(true);
        deckPlaceholder.SetActive(false);
    }

    public void HideRulesOverlay()
    {
        // TODO: animate these transitions
        mainMenuHUD.SetActive(true);
        rulesOverlay.SetActive(false);
        deckPlaceholder.SetActive(true);
    }

    public void HideMainMenu()
    {
        // Hide the main menu with a scale animation
        mainMenuHUD.transform.DOScale(0, 0.25f)
            .SetEase(Ease.OutSine);
    }

    public void ShowGameEndOverlay(int id)
    {
        gameEndOverlay.SetActive(true);

        if (id == -2)
        {
            // It's a draw - no more possible moves AND no more cards on the field
            winnerTxt.text = "No moves left! It's a draw!";
        }
        else
        {
            // A player has won
            winnerTxt.text = "Player " + (id + 1) + " wins!";
        }

        winnerTxt.transform.localPosition = new Vector3(winnerTxtPos.x - 500f, winnerTxtPos.y, winnerTxtPos.z);
        winnerTxt.transform.DOLocalMoveX(0f, 1f, true)
            .SetDelay(0.25f)
            .SetEase(Ease.OutElastic);
    }

    public void HideGameEndOverlay()
    {
        ingameOverlay.SetActive(false);
        gameEndOverlay.SetActive(false);

        // Ccale the main menu buttons again
        mainMenuHUD.transform.localScale = Vector3.one;

        turnArrow.SetActive(false);
    }

    public void ShowIngameOverlay()
    {
        ingameOverlay.SetActive(true);
    }

    public void ShowDrawCardText()
    {
        drawCardPromptTxt.gameObject.SetActive(true);
    }

    public void HideDrawCardText()
    {
        drawCardPromptTxt.gameObject.SetActive(false);
    }

}
