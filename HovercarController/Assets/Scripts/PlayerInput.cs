using UnityEngine;
// using Rewired;
using ReplacementInput;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private string _verticalAxisName = "Vertical";
    [SerializeField] private string _horizontalAxisName = "Horizontal";
    [SerializeField] private string _brakingKey = "Brake";
    [SerializeField] private string _boostingKey = "Boost";
    [SerializeField] private string _cancelButton = "Cancel";
    
    public float Thruster { get; private set; }
	public float Rudder { get; private set; }
	public bool IsBraking { get; private set; }
    public bool IsBoosting { get; private set; }

    // public int PlayerId = 0;
    private Player _inputPlayer;

    private void Awake()
    {
        _inputPlayer = new Player(); // ReInput.players.GetPlayer(PlayerId);
    }

    void Update()
	{
		//If the player presses the Escape key and this is a build (not the editor), exit the game
	    if (_inputPlayer.GetButtonDown(_cancelButton))
	        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()
	            .buildIndex);

		//Get the values of the thruster, rudder, and brake from the input class
		Thruster = Mathf.Clamp01(_inputPlayer.GetAxis(_verticalAxisName));
		Rudder = _inputPlayer.GetAxis(_horizontalAxisName);
		IsBraking = _inputPlayer.GetButton(_brakingKey);
	    IsBoosting = _inputPlayer.GetButton(_boostingKey);
	}
}
