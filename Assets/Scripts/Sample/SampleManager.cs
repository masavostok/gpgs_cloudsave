using UnityEngine;
using UnityEngine.UI;

public class SampleManager : MonoBehaviour
{
    public Text messageBox;
    public Button btnLoad;
    public Button btnSave;
    private GPGCloudStorage googlePlay;

    // Use this for initialization
    void Start ()
    {
        googlePlay = new GPGCloudStorage();
        //ファイル名
        googlePlay.SetFileName("tk_cloud_data_1");

        MessageReceiver("Ready.");

        //Invoke("Tmf", 0.5f);

        btnLoad.onClick.AddListener(() => {
            OnClickLoad();
        });

        btnSave.onClick.AddListener(() => {
            OnClickSave();
        });
    }

    //Cloud SAVE
    public void OnClickSave()
    {
        int r = UnityEngine.Random.Range(0, 1000);
        string msg = "save-data," + r.ToString() + "," +
                        System.Guid.NewGuid().ToString();
        MessageReceiver("Trying Save : " + msg);

        googlePlay.GPG_CloudSave(msg, (res) => {
            if (string.IsNullOrEmpty(res)) {
                MessageReceiver("Save NG ");
            }
            else {
                MessageReceiver("Save OK : " + res);
            }
        });
    }

    //Cloud LOAD
    public void OnClickLoad()
    {
        MessageReceiver("Trying Load...");

        googlePlay.GPG_CloudLoad((res) => {
            if (string.IsNullOrEmpty(res)) {
                MessageReceiver("Load NG :");
            }
            else {
                MessageReceiver("Load OK : " + res);
            }
        });
    }

    //Login
    public void Login()
    {
        if (!googlePlay.IsLoggedIn) {
            messageBox.text = "Connecting...";

            googlePlay.GPG_SignIn((success) => {
                if (success) {
                    MessageReceiver("Sign in OK");
                }
                else {
                    MessageReceiver("Sign in Faild");
                }
            });
        }
        else {
            messageBox.text = "No action";
        }
    }



    //
    public void MessageReceiver(string msg)
    {
        messageBox.text = msg + "\n" + messageBox.text;
        Debug.Log(msg);
    }
}
