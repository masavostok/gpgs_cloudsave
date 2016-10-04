using System;
#if (!UNITY_EDITOR || FORCE_GPG_ON) && !NO_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif
using UnityEngine;
using UnityEngine.SocialPlatforms;

/**
 * Google Play Games plugin for Unity
 * https://github.com/playgameservices/play-games-plugin-for-unity
 * 
 * GooglePlayGame クラウドセーブ機能
 * ※以下のメソッドを使ってください
 *  GPG_SignIn
 *  GPG_CloudSave
 *  GPG_CloudLoad
 *  [Usage]
 *  var gpg = new GPGCloudStorage();
 *  gpg.GPG_CloudSave("fileName", "data_string", (bool success) => { ... });
 *  gpg.GPG_CloudLoad("fileName", (string data) => { ... });
 */
public class GPGCloudStorage
{
    private bool active = false;
    private byte[] newDataRaw = null;
    private string _cloudDataFileName = "cloud_data_";
    //Social ID
    public string myGooglePlayId = "";

    
    //ファイル名の設定
    public void SetFileName(string name)
    {
        _cloudDataFileName = name;
    }

    public bool IsLoggedIn
    {
        get {
            return active;
        }
    }

#if (!UNITY_EDITOR || FORCE_GPG_ON) && !NO_GPGS

    /// <summary>
    /// Google+サインイン
    /// </summary>
    public void GPG_SignIn(Action<bool> onSignin)
    {
        SignIn("", (success) => onSignin(success));
    }

    /// <summary>
    /// 文字列をクラウド保存（無条件に上書きします）
    /// </summary>
    public void GPG_CloudSave(string fileName, string writeString, Action<string> onComp)
    {
        if (IsLoggedIn) 
        {
            SetFileName(fileName);
            Write(writeString, onComp);
        }
        else 
        {
            SignIn("", (success) => {
                SetFileName(fileName);
                Write(writeString, onComp);
            });
        }
    }

    /// <summary>
    /// クラウド保存した文字列をロード
    /// </summary>
    public void GPG_CloudLoad(string fileName, Action<string> onComp)
    {
        if (IsLoggedIn) 
        {
            SetFileName(fileName);
            Read(onComp);
        }
        else 
        {
            SignIn("", (success) => {
                SetFileName(fileName);
                Read(onComp);
            });
        }
    }

    /**
     * 初期化
     */
    private void Init()
    {
        var config = new PlayGamesClientConfiguration.Builder()
        .EnableSavedGames()
        .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
    }

    /**
     * サインイン
     */
    private bool SignIn(string name, Action<bool> onSignin)
    {
        if (!active) 
        {
            Init();
        }
        Social.localUser.Authenticate((bool success) => {
            active = success;
            myGooglePlayId = PlayGamesPlatform.Instance.GetUserId();
            onSignin(success);
        });
        return true;
    }

    /**
     * 書き込み
     */
    private void Write(string inputString, Action<string> onComplete)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(inputString);
        WriteUpdatedOrRead(onComplete, data);
    }

    /*
    private void Write(byte[] data, Action<string> onComplete)
    {
        WriteUpdatedOrRead(onComplete, data);
    }
    */

    /**
     * 読み込み
     */
    private void Read(Action<string> onComplete)
    {
        WriteUpdatedOrRead(onComplete);
    }

    private void WriteUpdatedOrRead(Action<string> onComplete, byte[] writeData = null)
    {
        // Local variable
        ISavedGameMetadata currentGame = null;//（２）で初期化
        newDataRaw = writeData;

        // SAVE CALLBACK: Handle the result of a write
        (SavedGameRequestStatus, game) writeCallback = 
        (SavedGameRequestStatus status, ISavedGameMetadata game) => 
        {
            if (status == SavedGameRequestStatus.Success) 
            {
                onComplete(System.Text.Encoding.UTF8.GetString(writeData));
            }
            else 
            {
                //エラー
                onComplete(null);
            }
        };

        // LOAD CALLBACK: Handle the result of a binary read
        (SavedGameRequestStatus, byte[]) readBinaryCallback = 
        (SavedGameRequestStatus status, byte[] data) => 
        {
            if (status == SavedGameRequestStatus.Success) 
            {
                // Read score from the Saved Game
                if (newDataRaw == null) 
                {
                    //Read Finish
                    //読み出し－Phase３
                    string readString = System.Text.Encoding.UTF8.GetString(data);
                    onComplete(readString);
                }
            }
            else
            {
                //エラー
                onComplete(null);
            }
        };

        ReadSavedGame(_cloudDataFileName, (SavedGameRequestStatus status, ISavedGameMetadata game) =>
        {
            currentGame = game;

            if (newDataRaw == null) 
            {
                //読み出し
                if (status == SavedGameRequestStatus.Success) 
                {
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, readBinaryCallback);
                }
                else 
                {
                    //エラー
                    onComplete(null);
                }
            }
            else 
            {
                //書き込み
                WriteSavedGame(currentGame, newDataRaw, writeCallback);
            }
        });
    }

    //Load From Cloud Saved Data
    private void ReadSavedGame(string filename,
                            Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(filename,
                                                            DataSource.ReadNetworkOnly, //キャッシュは読まない
                                                            ConflictResolutionStrategy.UseLongestPlaytime,
                                                            callback);
    }

    //Save Data To Cloud
    private void WriteSavedGame(ISavedGameMetadata game, byte[] savedData,
                            Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
            .WithUpdatedPlayedTime(TimeSpan.FromMinutes(game.TotalTimePlayed.Minutes + 1))
            .WithUpdatedDescription("Saved at: " + System.DateTime.Now);

        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, callback);
    }

    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.
        }
        else {
            // handle error
        }
    }


#else

    //For Debug

    public void GPG_SignIn(Action<bool> onSignin)
    {
        onSignin(false);
    }

    public void GPG_CloudLoad(Action<string> onComplete)
    {
        onComplete("");
    }

    public void GPG_CloudSave(string inputString, Action<string> onComplete)
    {
        onComplete(null);
    }

#endif

}
