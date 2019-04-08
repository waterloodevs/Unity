using Kin;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class KinController : MonoBehaviour
{
    KinClient kinClient;
    KinAccount kinAccount;
    public string publicAddress = "";

    //a random 4-digit appid for test environment
    string appID = "1234";

    public int currentFunds;
    public static KinController Instance;

    //send funds to this account on transaction
    string targetAddress = "GCTUZAUCGM4TVMMOOHJ5PBX6HFJUIEPUVGCACWNOKFF2TT3XHUXBL3FG";

    //reference to the main menu handler script in order to update coins
    public MenuHandler menuHandler;

    private void Awake()
    {
        //don't destroy this object when game starts
        DontDestroyOnLoad(gameObject);

        //set it as a singleton to be referenced by other scripts
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentFunds = -1;
        // Kin client is the manager for Kin accounts
        kinClient = new KinClient(Kin.Environment.Test, appID);

        try
        {
            //if user doesn't already have an account, create one and fund it
            if (!kinClient.HasAccount())
            {
                kinAccount = kinClient.AddAccount();
                publicAddress = kinAccount.GetPublicAddress();
                Debug.Log(kinAccount.GetPublicAddress());
                CreateAndFundAccount(100);
                menuHandler.UpdateCoins(100);
            }
            //if user already had an account, set the public address and retrieve it's balance
            else
            {
                kinAccount = kinClient.GetAccount();
                publicAddress = kinAccount.GetPublicAddress();
                Debug.Log(kinAccount.GetPublicAddress());
                kinAccount.GetBalance((ex, balance) =>
                {
                    if (ex == null)
                    {
                        Debug.Log("Balance is: " + balance);
                        currentFunds = (int) Decimal.Floor(balance);
                        menuHandler.UpdateCoins(currentFunds);
                    }
                    else
                        Debug.Log("Get balance failed. " + ex);
                
                });
            }

        }
        catch (Exception e)
        {
            Debug.Log("Account could not be created");
            Debug.LogError(e);
        }
    }
    
    /// <summary>
    /// function used to add further funds to an account.
    /// </summary>
    /// <param name="amount"></param>
    public void AddFunds(int amount)
    { 
        StartCoroutine(HandleAccount(KinActionString(1, amount), ifSuccessful => {
            Debug.Log("Funds added: " + ifSuccessful);
            if(ifSuccessful)
                currentFunds += amount;
        }));
    }

    /// <summary>
    /// function used to create an account and fund it. This is mainly used to override Kin initial funding of 10k
    /// </summary>
    /// <param name="amount"></param>
    void CreateAndFundAccount(int amount)
    {
        StartCoroutine(HandleAccount(KinActionString(0, amount), ifSuccessful => {
            Debug.Log("Account creation: " + ifSuccessful);
            if (ifSuccessful)
                currentFunds = amount;
        }));
    }

    /// <summary>
    /// corresponding string is return based on what 'type' of action is to be called. 
    /// 1 means that we are funding an already existing account, 
    /// 0 means we are creating and funding it
    /// used in addfunds and creatandfundaccount function
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    string KinActionString(int type, int amount)
    {
        if(type == 0)
            return "http://friendbot-testnet.kininfrastructure.com/?addr=" + publicAddress + "&amount=" + amount;
        
        return "http://friendbot-testnet.kininfrastructure.com/fund?addr=" + publicAddress + "&amount=" + amount;
        
    }

    /// <summary>
    /// function used to call a corresponing action string (create and fund an account, or just fund an already existing account)
    /// </summary>
    /// <param name="url"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    IEnumerator HandleAccount(string url, Action<bool> onComplete = null)
    {
        Debug.Log(url);
        var req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
            if (onComplete != null)
                onComplete(false);
        }
        else
        {
            Debug.Log("response code: " + req.responseCode);
            Debug.Log(req.downloadHandler.text);
            if (onComplete != null)
                onComplete(true);
        }
    }
    

    /// <summary>
    /// Function used to transfer kin amount to another account
    /// called when the user uses a revive
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="onSuccessful"></param>
    public void TransferKin(int amount, Action <bool> onSuccessful)
    {
        //we get a minimum fee as it is a necessary parameter in build transaction
        kinClient.GetMinimumFee((ex, fee) => {
            //upon successful retrievel of minimum fee, build a transaction with this fee
            if (ex == null)
            {
                Debug.Log("Minimun fee is " + fee);
                kinAccount.BuildTransaction(targetAddress, amount, fee, "Revive Kin", (ex2, transaction) =>
                {
                    if (ex2 == null)
                    {
                        Debug.Log("Build a transaction with result: " + transaction);
                        kinAccount.SendTransaction(transaction, (ex3, transactionID) =>
                        {
                            if (ex3 == null)
                            {
                                Debug.Log("Build a transaction with result: " + transactionID);
                                onSuccessful(true);
                            }
                            else
                            {
                                Debug.Log("Sending transaction failed! Error: " + ex2);
                                onSuccessful(false);
                            }
                        });
                    }
                    else
                    {
                        Debug.Log("Build transaction failed! " + ex2);
                    }
                });
        }});
    }
    
}
