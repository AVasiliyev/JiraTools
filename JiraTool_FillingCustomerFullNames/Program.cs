using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using JiraTool_FillingCustomerFullNames.Types;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace JiraTool_FillingCustomerFullNames
{
    class Program
    {
        static readonly AppSettingsReader MyReader = new AppSettingsReader();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string fileName = String.Empty;

        static void Main(string[] args)
        {
            GetFileName(args);
            Console.WriteLine($"---- Processed file: {fileName}");

            var urlTemplate = GetUrl() + "/admin/jira-service-desk/portal-only-customers/view?username={0}";
            var usersList = GetUsersList(urlTemplate);
            FillData(usersList);
        }

        private static void GetFileName(string[] args)
        {
            if (args.Length == 1)
            {
                fileName = args[0];
                return;
            }

            try
            {
                fileName = MyReader.GetValue("FileName", typeof(string)).ToString();
            }
            catch (Exception)
            {
                fileName = "UsersList.txt";
            }
        }

        private static void FillData(List<User> usersList)
        {
            using (IWebDriver driver = new FirefoxDriver())
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                Login(driver, wait);
                usersList.ForEach(d => FillUserName(d, driver, wait));
            }
        }

        private static void FillUserName(User user, IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                driver.Navigate().GoToUrl(user.Url);
                
                // wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".sc-kpOJdX.fVGRSc")));
                var alertWindowButton = driver.FindElements(By.XPath("//span[contains(text(), 'Ok, got it')]"));

                if(alertWindowButton.Count > 0)
                    alertWindowButton[0].Click();

                driver.SwitchTo().Frame(driver.FindElement(By.CssSelector("iframe")));

                // wait.Until(ExpectedConditions.ElementExists(By.CssSelector("button.aui-button.jsd-portal-only-customers-profile-edit")));
                // driver.FindElement(By.CssSelector("button.aui-button.jsd-portal-only-customers-profile-edit")).Click();
                wait.Until(ExpectedConditions.ElementExists(By.XPath("//button[.='Edit']")));
                driver.FindElement(By.XPath("//button[.='Edit']")).Click();

                var elm = driver.FindElement(By.XPath("//input"));
                elm.Clear();
                elm.SendKeys(user.FullName);

                driver.FindElement(By.XPath("//button[.='Save']")).Click();

                logger.Info($"{user.Email} - Ok");
            }
            catch (Exception e)
            {
               logger.Error($"{user.Email} - Error");
            }
        }

        private static void Login(IWebDriver driver, WebDriverWait wait)
        {
            var login = MyReader.GetValue("Login", typeof(string)).ToString();
            var password = MyReader.GetValue("Password", typeof(string)).ToString();

            driver.Navigate().GoToUrl(GetUrl());
            // driver.FindElement(By.Id("user-options")).Click();
            driver.FindElement(By.Id("username")).SendKeys(login);
            driver.FindElement(By.Id("login-submit")).Click();
            
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("password")));
            driver.FindElement(By.Id("password")).SendKeys(password);

            driver.FindElement(By.Id("login-submit")).Click();

            wait.Until(ExpectedConditions.ElementExists(By.CssSelector("#admin_menu")));
        }

        private static List<User> GetUsersList(string urlTemplate)
        {
            var path = $@"{Directory.GetCurrentDirectory()}\Data\{fileName}";
            var lines = File.ReadAllLines(path);
            
            return lines.Select(line => line.Split(',')).Select(data => new User(
                data[0], 
                data[1], 
                string.Format(urlTemplate, WebUtility.UrlEncode(data[0])))).ToList();
        }

        private static string GetUrl()
        {
            return MyReader.GetValue("SiteUrl", typeof(string)).ToString();
        }
    }
}
