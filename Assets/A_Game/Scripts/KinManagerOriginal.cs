﻿using Kin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    public class KinManagerOriginal:SingletonMonobehaviour<KinManagerOriginal>, IPaymentListener, IBalanceListener, IAccountCreationListener
    {
        [SerializeField] private string _appId = "1acd";  //Needs to be 4 characters 
        [SerializeField] private string _storeKey = "myStoreKey";

        private KinClient _kinClient;
        private KinAccount _kinAccount;  //Multiple can be created using AddAccount
        public bool _isNew = false;
        public bool _isAccountCreated = false;

        void Awake()
        {
            _kinClient = new KinClient(Kin.Environment.Test, _appId, _storeKey);
            
            StartCoroutine(StartKin());
        }

        IEnumerator StartKin()
        {
            AddAccount();

            yield return StartCoroutine(CreateAccount());

            yield return StartCoroutine(CheckAccountStatus(AccountStatus.Created));
            
        }

        void OnDestroy()
        {
            _kinAccount.RemovePaymentListener(this);
            _kinAccount.RemoveBalanceListener(this);
            _kinAccount.RemoveAccountCreationListener(this);
        }

        public void OnEvent(PaymentInfo payment)
        {
            Debug.Log("On Payment: " + payment);
        }

        public void OnEvent(decimal balance)
        {
            Debug.Log("On Balance: " + balance);
        }

        public void OnEvent()
        {
            Debug.Log("On Account Created");
        }

        public void AddAccount()
        {
            try
            {
                if (!_kinClient.HasAccount())
                {
                    _kinAccount = _kinClient.AddAccount();
                    _kinAccount.AddAccountCreationListener(this);
                    _isNew = true;
                    //yield return StartCoroutine( CheckAccountStatus( AccountStatus.Created ) );
                    
                } else
                {
                    _kinAccount.AddPaymentListener(this);
                    _kinAccount.AddBalanceListener(this);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void DeleteAccount(int accountIndex = 0)
        {
            _kinClient.DeleteAccount(accountIndex);
        }

        public KinAccount RetrieveKinAccount()
        {
            if (_kinClient.HasAccount())
            {
                _kinAccount = _kinClient.GetAccount(0);
                return _kinAccount;
            }

            return null;
        }

        protected IEnumerator CreateAccount()
        {
            yield return StartCoroutine(KinOnboarding.CreateAccount(_kinAccount.GetPublicAddress(), OnCompleteCreateAccount));
        }

        void OnCompleteCreateAccount(bool didSucceed)
        {
            _isAccountCreated = true;
            Debug.Log("Account Created! - " + _kinAccount.GetPublicAddress());
            _kinAccount.AddPaymentListener(this);
            _kinAccount.AddBalanceListener(this);
        }

        protected IEnumerator CheckAccountStatus(AccountStatus statusShouldBe)
        {
            var hasResult = false;
            _kinAccount.GetStatus((ex, status) =>
            {
                hasResult = true;
            });

            yield return new WaitUntil(() => hasResult);
        }

        public string GetAccountPublicAddress()
        {
            return _kinAccount.GetPublicAddress();
        }

        public AccountStatus GetAccountStatus()
        {
            AccountStatus myStatus = AccountStatus.NotCreated; 

            _kinAccount.GetStatus((ex, status) =>
            {
                if (ex == null)
                {
                    myStatus = status;
                    Debug.Log("Account status: " + status);
                }
                else
                    Debug.LogError("Get Account Status Failed. " + ex);
            });

            return myStatus;
        }

        public decimal GetAccountBalance()
        {
            decimal accountBalance = 0;

            _kinAccount.GetBalance((ex, balance) =>
            {
                if (ex == null)
                {
                    accountBalance = balance;
                    Debug.Log("Balance: " + balance);
                }
                else
                    Debug.LogError("Get Balance Failed. " + ex);
            });

            return accountBalance;
        }

        public void TransferKinToAnotherAccountPaid(string toAddress, decimal amountInKin, int fee)
        {
            // we could use here some custom fee or we can can call the blockchain in order to retrieve
            // the current minimum fee by calling kinClient.getMinimumFee(). Then when you get the minimum
            // fee returned and you can start the 'send transaction flow' with this fee.
            _kinAccount.BuildTransaction(toAddress, amountInKin, fee, (buildException, transaction) =>
            {
                if (buildException == null)
                {
                    // Here we already got a Transaction object before actually sending the transaction. This means
                    // that we can, for example, send the transaction id to our servers or save it locally  
                    // in order to use it later. For example if we lose network just after sending 
                    // the transaction then we will not know what happened with this transaction. 
                    // So when the network is back we can check what is the status of this transaction.
                    Debug.Log("Build Transaction result: " + transaction);
                    _kinAccount.SendTransaction(transaction, (sendException, transactionId) =>
                    {
                        if (sendException == null)
                            Debug.Log("Send Transaction result: " + transactionId);
                        else
                            Debug.LogError("Send Transaction Failed. " + sendException);
                    });
                }
                else
                {
                    Debug.LogError("Build Transaction Failed. " + buildException);
                }
            });
        }

        //public void TransferKinToAnotherAccountFree(string toAddress, decimal amountInKin, int fee)
        //{
        //    _kinAccount.BuildTransaction(toAddress, amountInKin, fee, (buildException, transaction) =>
        //    {
        //        if (buildException == null)
        //        {
        //            Debug.Log("Build Transaction result: " + transaction);

        //            var whitelistTransaction = YourWhitelistService.WhitelistTransaction(transaction);  //Irving: Not sure where to get this service yet.
        //            _kinAccount.SendWhitelistTransaction(transaction.Id, whitelistTransaction, (sendException, transactionId) =>
        //            {
        //                if (sendException == null)
        //                    Debug.Log("Send Transaction result: " + transactionId);
        //                else
        //                    Debug.LogError("Send Transaction Failed. " + sendException);
        //            });
        //        }
        //        else
        //        {
        //            Debug.LogError("Build Transaction Failed. " + buildException);
        //        }
        //    });
        //}

        public void TransferKinToAnotherAccountPaidWithMemo(string toAddress, decimal amountInKin, int fee, string memo)
        {
            _kinAccount.BuildTransaction(toAddress, amountInKin, fee, memo, (buildException, transaction) =>
            {
                if (buildException == null)
                {
                    // Here we already got a Transaction object before actually sending the transaction. This means
                    // that we can, for example, send the transaction id to our servers or save it locally  
                    // in order to use it later. For example if we lose network just after sending 
                    // the transaction then we will not know what happened with this transaction. 
                    // So when the network is back we can check what is the status of this transaction.
                    Debug.Log("Build Transaction result: " + transaction);
                    _kinAccount.SendTransaction(transaction, (sendException, transactionId) =>
                    {
                        if (sendException == null)
                            Debug.Log("Send Transaction result: " + transactionId);
                        else
                            Debug.LogError("Send Transaction Failed. " + sendException);
                    });
                }
                else
                {
                    Debug.LogError("Build Transaction Failed. " + buildException);
                }
            });
        }

        public void BackupAccount()
        {
            _kinAccount.BackupAccount(_kinClient,
               (KinException ex, BackupRestoreResult result) =>
               {
                   switch (result)
                   {
                       case BackupRestoreResult.Success:
                           Debug.Log("Account backed up successfully");
                           break;
                       case BackupRestoreResult.Cancel:
                           Debug.Log("Account backup canceled");
                           break;
                       case BackupRestoreResult.Failed:
                           Debug.Log("Account backup failed");
                           Debug.LogError(ex);
                           break;
                   }
               });
        }
    }
}