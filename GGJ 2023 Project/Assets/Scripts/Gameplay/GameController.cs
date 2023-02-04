using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class GameController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI energyText;

    // Nutrients + Water from the soil + sunlight = energy in different amounts.
    public double Energy { get; set; }
    public double startingEnergy = 100;
    public double energyDecay = 0.1;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        Energy -= energyDecay * Time.deltaTime;

        energyText.text = $"Energy: {Energy}";
        if (Energy <= 0) {
            // Game over
            // TODO: more elaborate sequence or animation, for now just scene change.
            SceneManager.LoadScene("GameOverScene");
        }
    }

    void StartGame() {
        Energy = startingEnergy;
    }
}
