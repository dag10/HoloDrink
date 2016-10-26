using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start()
    {
        System.Action showDrinksAction = () =>
        {
            this.BroadcastMessage("OnShowDrinks");
        };

        System.Action hideDrinksAction = () =>
        {
            this.BroadcastMessage("OnHideDrinks");
        };

        keywords.Add("I'm thirsty", showDrinksAction);
        keywords.Add("These pretzels are making me thirsty", showDrinksAction);

        keywords.Add("Dismiss drinks", hideDrinksAction);
        keywords.Add("Take them away boys", hideDrinksAction);
        keywords.Add("Bake them away toys", hideDrinksAction);
        keywords.Add("Bake em away toys", hideDrinksAction);
        keywords.Add("Actually I am no longer thirsty but thank you very much for providing me with this opportunity", hideDrinksAction);

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}