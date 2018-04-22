﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data.Entity;

namespace BankingSystem
{
    public class Connector
    {

        private static bank_systemEntities dbContext;

        protected static void connectDBContext() //singletone
        {
            if (dbContext == null)
                dbContext = new bank_systemEntities();
        }

        protected List<user> getUsersToList()
        {
            return dbContext.user.ToList();
        }

        protected List<data_of_user> getUsersDataToList()
        {
            return dbContext.data_of_user.ToList();
        }

        protected List<bank_account> getBankAccountsToList()
        {
            return dbContext.bank_account.ToList();
        }

        protected List<bank_deposit> getBankDepositsToList()
        {
            return dbContext.bank_deposit.ToList();
        }

        protected List<credit> getCreditsToList()
        {
            return dbContext.credit.ToList();
        }

        protected List<credit_type> getCreditTypesToList()
        {
            return dbContext.credit_type.ToList();
        }

        protected List<deposite_type> getDepositTypesToList()
        {
            return dbContext.deposite_type.ToList();
        }

        protected void addUser(user user)
        {
            try
            {
                dbContext.user.Add(user);
                dbContext.SaveChanges();
            }
            catch(MySqlException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        protected user findUserbyLogin(string userLogin)
        {
            return dbContext.user.Where(l => l.Login == userLogin).FirstOrDefault(); //находим по логину или возвращаем null
        }

        protected void updateUser(user user) //the login is not changing
        {
            try
            {
                user foundUser = findUserbyLogin(user.Login);
                if (foundUser != null) //если нашли
                {
                    //изменить юзера
                    foundUser.Password = user.Password;
                    foundUser.Admin = user.Admin;
                    // Обновить данные в БД с помощью Entity Framework
                    try
                    {
                        dbContext.Entry<user>(foundUser).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                    catch (MySqlException e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                dbContext.SaveChanges();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        protected void deleteUser(user user)
        {
            try
            {
                dbContext.user.Remove(user);
                dbContext.SaveChanges();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}