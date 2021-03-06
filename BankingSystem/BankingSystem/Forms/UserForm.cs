﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BankingSystem.BusinessLogic;

namespace BankingSystem.Forms
{
    public partial class UserForm : Form
    {
        UserContext userContext;
        user user;

        public UserForm(UserContext userContext, user user)
        {
            InitializeComponent();
            this.userContext = userContext;
            this.user = user;
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            labelSurname.Text = user.data_of_user.Surname;
            labelName.Text = user.data_of_user.Name;
            labelPatronymic.Text = user.data_of_user.Patronymic;
            labelPassSer.Text = user.data_of_user.Passport_series;
            labelPassNum.Text = user.data_of_user.Passport_number;

            user.bank_account = userContext.GetBankAccountsToBindingList(user.Login);

            dataGridViewAccounts.DataSource = user.bank_account;
            dataGridViewAccounts.Columns[0].Visible = false;
            dataGridViewAccounts.Columns[2].Visible = false;
            dataGridViewAccounts.Columns[4].Visible = false;
            dataGridViewAccounts.Columns[5].Visible = false;

            user.credit = userContext.GetCreditsToBindingList(user.Login);
            foreach(credit credit in user.credit)
            {
                double debt = CreditRegulator.СalculateDebt(credit);
                if ((debt > 0) && CreditRegulator.CheckImpositionOfFine(credit))
                {
                    MessageBox.Show("You are in arrears on the loan! You are fined in the amount of " + CreditRegulator.ImposeAFine(credit, debt).ToString());
                    userContext.UpdateUser(user);
                }
            }

            dataGridViewCredits.DataSource = user.credit;
            dataGridViewCredits.Columns[0].Visible = false;
            dataGridViewCredits.Columns[1].Visible = false;
            dataGridViewCredits.Columns[2].Visible = false;
            dataGridViewCredits.Columns[7].Visible = false;
            dataGridViewCredits.Columns[8].Visible = false;

            viewUserDeposits();

            BindingList<bank_account> blAccountsWithoutDeposits = new BindingList<bank_account>(user.bank_account.Where(a => a.bank_deposit == null).ToList());
            dataGridViewAccountsWithoutDeposits.DataSource = blAccountsWithoutDeposits;
            dataGridViewAccountsWithoutDeposits.Columns[0].Visible = false;
            dataGridViewAccountsWithoutDeposits.Columns[2].Visible = false;
            dataGridViewAccountsWithoutDeposits.Columns[4].Visible = false;
            dataGridViewAccountsWithoutDeposits.Columns[5].Visible = false;

            List<bank_deposit> userDeposits = user.bank_account.Where(a => a.bank_deposit != null).Select(d => d.bank_deposit).ToList();
            foreach (bank_deposit deposit in userDeposits)
            {
                if (AccountsAndDepositsRegulator.InterestAccrual(deposit))
                {
                    userContext.DeleteBankDeposit(deposit);
                    MessageBox.Show("The deposit period has expired! Deposit is closed! The account number of the deposit: " + deposit.bank_account.Number);
                }
                userContext.UpdateUser(user);
                viewUserDeposits();
            }
        }

        private void viewUserDeposits()
        {
            dataGridViewDeposits.DataSource = userContext.GetBankDepositsToBindingList(user.Login);
            dataGridViewDeposits.Columns[0].Visible = false;
            dataGridViewDeposits.Columns[2].Visible = false;
            dataGridViewDeposits.Columns[5].Visible = false;
            dataGridViewDeposits.Columns[6].Visible = false;
        }

        private void buttonOpenAccount_Click(object sender, EventArgs e)
        {
            bank_account newAccount = AccountsAndDepositsRegulator.GenerateNewAccount();
            newAccount.user = user;
            user.bank_account.Add(newAccount);
            userContext.UpdateUser(user);
            MessageBox.Show("The account added!");
        }

        private void buttonCloseAccount_Click(object sender, EventArgs e)
        {
            if (dataGridViewAccounts.SelectedRows.Count > 0)
            {
                int selectedAccountId = Convert.ToInt32(dataGridViewAccounts.SelectedRows[0].Cells[0].Value);
                bank_account deletingAccount = user.bank_account.Where(a => a.id == selectedAccountId).First();

                if (AccountsAndDepositsRegulator.DeleteAccountCheck(deletingAccount))
                {
                    userContext.DeleteBankAccount(deletingAccount);
                    userContext.UpdateUser(user);
                    MessageBox.Show("The account is deleted!");
                }
            }
            else
                MessageBox.Show("Account is not selected!");
        }

        private void buttonTransfer_Click(object sender, EventArgs e)
        {
            transferMoney(false);
            dataGridViewAccounts.Refresh();
        }

        private void transferMoney(bool toAnotherUser)
        {
            object[] accounts = user.bank_account.Where(a => a.bank_deposit == null).Select(a => a.Number).ToArray();

            MoneyTransferForm transferForm = new MoneyTransferForm(accounts, toAnotherUser);
            if (transferForm.ShowDialog() == DialogResult.OK)
            {
                string numberAccountFrom = transferForm.comboBoxFrom.Text;
                string numberAccountTo;
                if (toAnotherUser)
                    numberAccountTo = transferForm.textBoxToAccount.Text.Trim();
                else
                    numberAccountTo = transferForm.comboBoxTo.Text;

                int sum;

                if (numberAccountFrom != null && numberAccountTo != null)
                {
                    if (transferForm.textBoxSum.Text != "" && int.TryParse(transferForm.textBoxSum.Text, out sum))
                    {
                        bank_account accountFrom = user.bank_account.Where(a => a.Number == numberAccountFrom).First();
                        bank_account accountTo;
                        if (toAnotherUser)
                            accountTo = userContext.GetBankAccountsToList().Where(a => a.Number == numberAccountTo).FirstOrDefault();
                        else
                            accountTo = user.bank_account.Where(a => a.Number == numberAccountTo).FirstOrDefault();
                        if (accountTo != null)
                        {
                            if (AccountsAndDepositsRegulator.TransferMoney(accountFrom, accountTo, sum))
                            {
                                userContext.UpdateUser(user);
                                MessageBox.Show("The money was transferred!");
                            }
                        }
                        else
                        {
                            if (toAnotherUser)
                                MessageBox.Show("The payee account entered is not exist!");
                            else
                                MessageBox.Show("The account is not selected!");
                        }

                    }
                    else
                        MessageBox.Show("The sum entered is not correct!");
                }
                else
                    MessageBox.Show("The account is not selected!");
            }
        }

        private void buttonOpenDeposit_Click(object sender, EventArgs e)
        {
            BindingList<deposite_type> types = userContext.GetDepositTypesToBindingList();
            TypesOfDepositsForm typesFotm = new TypesOfDepositsForm(types, user.bank_account.Select(a => a.Number).ToArray());
            if (typesFotm.ShowDialog() == DialogResult.OK)
            {
                double sum;
                if (typesFotm.textBoxSum != null && double.TryParse(typesFotm.textBoxSum.Text, out sum))
                {
                    sum = Math.Round(sum, 1);
                    string numberAccountFrom = typesFotm.comboBoxFrom.Text;
                    if (numberAccountFrom != "")
                    {
                        bank_account accountFrom = user.bank_account.Where(a => a.Number == numberAccountFrom).First();
                        int depositTypeId = Convert.ToInt32(typesFotm.dataGridViewTypesOfDeposits.SelectedRows[0].Cells[0].Value);
                        deposite_type depositType = userContext.FindDepositeTypeById(depositTypeId);
                        bank_account depositAccount = AccountsAndDepositsRegulator.OpenDeposite(depositType, accountFrom, sum);
                        if (depositAccount != null)
                        {
                            depositAccount.user = user;
                            user.bank_account.Add(depositAccount);
                            userContext.UpdateUser(user);
                            viewUserDeposits();
                            MessageBox.Show("Deposit is opened!");
                        }
                    }
                    else
                        MessageBox.Show("The account is not selected!");
                }
                else
                    MessageBox.Show("The entered sum is incorrect!");
            }
        }

        private void buttonCloseDeposit_Click(object sender, EventArgs e)
        {
            if (dataGridViewDeposits.SelectedRows.Count > 0)
            {
                int selectedDepositAccountId = Convert.ToInt32(dataGridViewDeposits.SelectedRows[0].Cells[0].Value);
                bank_deposit closingDeposit = userContext.FindDepositeByAccountId(selectedDepositAccountId);
                if (AccountsAndDepositsRegulator.CloseDepositCheck(closingDeposit))
                {
                    userContext.DeleteBankDeposit(closingDeposit);
                    userContext.UpdateUser(user);
                    viewUserDeposits();
                    MessageBox.Show("Deposit is closed!");
                }
                else
                    MessageBox.Show("A deposit without the possibility of early closure can not be closed until the expiration date!");
            }
            else
                MessageBox.Show("Deposit is not selected!");

        }

        private void buttonViewDepositInfo_Click(object sender, EventArgs e)
        {
            if (dataGridViewDeposits.SelectedRows.Count > 0)
            {
                int selectedDepositAccountId = Convert.ToInt32(dataGridViewDeposits.SelectedRows[0].Cells[0].Value);
                bank_deposit selectedDeposit = userContext.FindDepositeByAccountId(selectedDepositAccountId);
                InfoAboutDepositForm infoForm = new InfoAboutDepositForm(selectedDeposit);
                infoForm.Show();
            }
            else
                MessageBox.Show("Deposit is not selected!");
        }

        private void buttonTransferToAnotherUser_Click(object sender, EventArgs e)
        {
            transferMoney(true);
            dataGridViewAccountsWithoutDeposits.Refresh();
        }

        //credits!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        private void buttonViewInfoAboutCredit_Click(object sender, EventArgs e)
        {
            if (dataGridViewCredits.SelectedRows.Count > 0)
            {
                int selectedCreditId = Convert.ToInt32(dataGridViewCredits.SelectedRows[0].Cells[0].Value);
                credit selectedCredit = user.credit.Where(c => c.id == selectedCreditId).FirstOrDefault();
                InfoAboutCreditForm infoForm = new InfoAboutCreditForm(selectedCredit);
                infoForm.Show();
            }
            else
                MessageBox.Show("Credit is not selected!");
        }

        private void buttonMakePayment_Click(object sender, EventArgs e)
        {
            if (dataGridViewCredits.SelectedRows.Count > 0)
            {
                object[] accounts = user.bank_account.Where(a => a.bank_deposit == null).Select(a => a.Number).ToArray();
                int selectedCreditId = Convert.ToInt32(dataGridViewCredits.SelectedRows[0].Cells[0].Value);
                credit selectedCredit = user.credit.Where(c => c.id == selectedCreditId).FirstOrDefault();
                double debt = CreditRegulator.СalculateDebt(selectedCredit);
                double recommendedPay = CreditRegulator.CalculateRecommendedSumOfPayment(selectedCredit) + debt;
                double fullyRepaySum = CreditRegulator.CalculateSumForEarlyPayment(selectedCredit);

                CreditPayForm payForm = new CreditPayForm(accounts, debt, recommendedPay, fullyRepaySum);
                if (payForm.ShowDialog() == DialogResult.OK)
                {
                    string numberAccountFrom = payForm.comboBoxFrom.Text;
                    if (numberAccountFrom != "")
                    {
                        double sum;
                        if (payForm.textBoxSum != null && double.TryParse(payForm.textBoxSum.Text, out sum))
                        {
                            sum = Math.Round(sum, 1);
                            bank_account accountFrom = user.bank_account.Where(a => a.Number == numberAccountFrom).First();
                            if (CreditRegulator.PayForCredit(accountFrom, sum, selectedCredit))
                            {
                                userContext.UpdateUser(user);
                                dataGridViewCredits.Refresh();
                            }
                        }
                        else
                            MessageBox.Show("The sum entered is not correct!");
                    }
                    else
                        MessageBox.Show("The account is not selected!");
                }
                else
                    MessageBox.Show("Credit is not selected!");
            }
        }
    }
}
