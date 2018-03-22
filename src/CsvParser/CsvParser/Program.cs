using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvParser
{
    public enum OperationTypes
    {
        None,
        Resegmentation,
        PricingManualOverride,
        RecalculateLimits,
        WhitelistUpdate,
        WhitelistRemoval,
        CreditLimitUpdatePayments,
        CreditLimitUpdateRisk,
        BlackListBulkUpload,
        BlackListUpsert,
        AccountLock,
        Select,
        MarkBikError,
        RecalculateLimitsIL,
        UpdateIncome
    }

    class Program
    {
        private static Dictionary<string, string> Accounts { get; set; }
        private static List<Tuple<string, string>> BlackListValues { get; set; }
        private static List<Tuple<string, string, string, string>> AccountSegmentValues { get; set; }
        private static List<Tuple<string, string, string>> ILOnlyLimitsValues { get; set; }
        public static OperationTypes OperationType { get; set; }
        
        static void Main(string[] args)
        {
            LoadFile();
            CreateScript();
        }

        private static void CreateScript()
        {
            string header = "", values = "", filename = "", bottom = "";
            var singleHeader = true;
            var batch = 1000;
            switch (OperationType)
            {
                case OperationTypes.Resegmentation:
                    header = "INSERT INTO [Creditlimit].[creditlimit].[AccountDataToModify] (AccountId) VALUES";
                    values = "(\'{0}\'),";
                    filename = "AccDataToModify.sql";
                    break;
                case OperationTypes.PricingManualOverride:
                    header = "INSERT INTO [PricingManager].[pricingmanager].[ManualPricingRecalculation] (AccountId, BasePricingModelId, Multiplier, Shift) VALUES";
                    values = "(\'{0}\', {1}, {2}, {3}),";
                    filename = "AccountsPricings.sql";
                    break;
                case OperationTypes.RecalculateLimits:
                    header = "INSERT INTO #accountAmounts (AccountId, StlMax, StlMin, IlMax, IlMin, MaxInstallments, HistoryId, CreatedOn) VALUES ";
                    values = "(\'{0}\', {1}, 50, {2}, 50, {3}, NEWID(), GETDATE()),";
                    filename = "AccountsAmountsLLIL.sql";
                    batch = 1000;
                    break;
                case OperationTypes.RecalculateLimitsIL:
                    header = "INSERT INTO #accountAmounts (AccountId, IlMax, IlMin, MaxInstallments, HistoryId, CreatedOn) VALUES ";
                    values = "(\'{0}\', {1}, 50, {2}, NEWID(), GETDATE()),";
                    filename = "AccountsAmountsILOnly.sql";
                    batch = 1000;
                    break;
                case OperationTypes.WhitelistUpdate:
                    header = "UPDATE [Payments].[payment].[InstallmentLoanWhitelistedAccounts] SET [MaxInstallments] = {1} WHERE [AccountId] = \'{0}\'";
                    filename = "InstallmentUpdate.sql";
                    singleHeader = false;
                    break;
                case OperationTypes.WhitelistRemoval:
                    header = "DELETE FROM [Payments].[payment].[InstallmentLoanWhitelistedAccounts] WHERE AccountId in (";
                    values = "\'{0}\',";
                    bottom = ")";
                    filename = "InstallmentWhitelistRemoval.sql";
                    break;
                case OperationTypes.CreditLimitUpdatePayments:
                    header = "UPDATE [Payments].[payment].[AccountPreferences] SET [CreditLimit] = {1} WHERE [AccountId] = \'{0}\'";
                    filename = "CreditLimitUpdatePayments.sql";
                    singleHeader = false;
                    break;
                case OperationTypes.CreditLimitUpdateRisk:
                    header = "UPDATE [Risk].[risk].[RiskAccounts] SET [CreditLimit] = {1} WHERE [AccountId] = \'{0}\'";
                    filename = "CreditLimitUpdateRisk.sql";
                    singleHeader = false;
                    break;
                case OperationTypes.BlackListBulkUpload:
                    header = @"<Messages>";
                    values = "<CsAddBlackListRecord><Type>FullMatch</Type><FirstName>{0}</FirstName><LastName>{1}</LastName></CsAddBlackListRecord>";
                    bottom = "</Messages>";
                    filename = "CsAddBlackListRecord.txt";
                    batch = 1000;
                    break;
                case OperationTypes.BlackListUpsert:
                    header = "INSERT INTO @NewAmounts ([AccountId], [Amount]) VALUES ";
                    values = "(\'{0}\', {1}),";
                    filename = "CsAddBlackListRecord.sql";
                    break;
                case OperationTypes.AccountLock:
                    header = "INSERT INTO [Ops].[ops].[LockedAccounts] ([AccountId], [IsLockedTemporarily], [IsLockedPermanently], [LockedOn], [IsSoldToDCA]) VALUES";
                    values = "(\'{0}\', 0, 1, \'" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\', 1),";
                    filename = "AccountLock.sql";
                    break;
                case OperationTypes.Select:
                    header = "Select AccountId, MaxInstallments from PL_WongaHDS.payment.[InstallmentLoanWhitelistedAccounts] where AccountId in (";
                    values = "\'{0}\', ";
                    filename = "SelectMaxInstallments.sql";
                    bottom = ")";
                    batch = 100000;
                    break;
                case OperationTypes.MarkBikError:
                    //header
                    values = "UPDATE Bik.bik.BikReport SET NoError = 0 where CorrelationId = (select CorrelationId from Bik.bik.BikBureauReport where AccountId = \'{0}\' and ApplicationId = \'{1}\')"; 
                    filename = "UpdateBik.sql";
                    break;
                case OperationTypes.UpdateIncome:
                    header = "UPDATE [Risk].[risk].[EmploymentDetails] SET [NetMonthlyIncome] = {1} WHERE [AccountId] = \'{0}\'";
                    filename = "IncomeUpdate.sql";
                    singleHeader = false;
                    break;
            }
            
            var result = new StringBuilder();
            var lineCounter = 0;
            var batchCounter = 1;
            var newBatch = true;

            if (OperationType != OperationTypes.BlackListBulkUpload)
            {
                result.AppendLine("SET NOCOUNT ON;");
                result.AppendLine("SET XACT_ABORT ON;");
                result.AppendLine("BEGIN TRY");
                result.AppendLine("BEGIN TRANSACTION");
            }

            if (OperationType == OperationTypes.BlackListBulkUpload || OperationType == OperationTypes.BlackListUpsert)
            {
                foreach (var blackListValue in BlackListValues)
                {
                    if (newBatch && singleHeader)
                    {
                        result.AppendLine(header);
                        newBatch = false;
                    }

                    var value1 = blackListValue.Item1;
                    var value2 = blackListValue.Item2;
                    string line;
                    if (OperationType == OperationTypes.BlackListUpsert)
                    {
                        value1 = value1.Replace("'", "''");
                        value2 = value2.Replace("'", "''");
                        line = string.Format(values, value1, value2);
                    }
                    else
                    {
                        line = string.Format(values, value1, value2);
                    }
                    
                    result.AppendLine(line);
                    lineCounter++;

                    if (lineCounter == batchCounter * batch)
                    {
                        //if (singleHeader)
                        if (OperationType == OperationTypes.BlackListUpsert)
                            result = result.Remove(result.Length - 3, 1);
                        result.AppendLine(bottom);
                        newBatch = true;
                        batchCounter++;
                    }
                }
            }
            if (OperationType == OperationTypes.RecalculateLimitsIL)
            {
                foreach (var accountSegments in ILOnlyLimitsValues)
                {
                    if (newBatch && singleHeader)
                    {
                        result.AppendLine(header);
                        newBatch = false;
                    }

                    var value1 = accountSegments.Item1;
                    var value2 = accountSegments.Item2;
                    var value3 = accountSegments.Item3;

                    value1 = value1.Replace("'", "''");
                    value2 = value2.Replace("'", "''");
                    value3 = value3.Replace("'", "''");

                    var line = string.Format(values, value1, value2, value3);

                    result.AppendLine(line);
                    lineCounter++;

                    if (lineCounter == batchCounter * batch)
                    {
                        result = result.Remove(result.Length - 3, 1);
                        result.AppendLine(bottom);
                        newBatch = true;
                        batchCounter++;
                    }
                }
            }
            if (OperationType == OperationTypes.PricingManualOverride || OperationType == OperationTypes.RecalculateLimits)
            {
                foreach (var accountSegments in AccountSegmentValues)
                {
                    if (newBatch && singleHeader)
                    {
                        result.AppendLine(header);
                        newBatch = false;
                    }

                    var value1 = accountSegments.Item1;
                    var value2 = accountSegments.Item2;
                    var value3 = accountSegments.Item3;
                    var value4 = accountSegments.Item4;

                    value1 = value1.Replace("'", "''");
                    value2 = value2.Replace("'", "''");
                    value3 = value3.Replace("'", "''");
                    value4 = value4.Replace("'", "''");

                    var line = string.Format(values, value1, value2, value3, value4);
                   
                    result.AppendLine(line);
                    lineCounter++;

                    if (lineCounter == batchCounter * batch)
                    {
                        result = result.Remove(result.Length - 3, 1);
                        result.AppendLine(bottom);
                        newBatch = true;
                        batchCounter++;
                    }
                }
            }
            else
            {
                foreach (var account in Accounts)
                {
                    if (newBatch && singleHeader)
                    {
                        result.AppendLine(header);
                        newBatch = false;
                    }

                    var accountId = account.Key;
                    var value = account.Value;

                    //if (OperationType == OperationTypes.BlackListUpsert)
                    //{
                    //    value = Guid.NewGuid().ToString();
                    //}

                    string line;
                    if (value != null && !string.IsNullOrEmpty(values))
                    {
                        line = string.Format(values, accountId, value);
                    }
                    else if (value == null && !string.IsNullOrEmpty(values))
                    {
                        line = string.Format(values, accountId);
                    }
                    else
                    {
                        line = string.Format(header, accountId, value);
                    }
                    
                    result.AppendLine(line);
                    lineCounter++;

                    if (lineCounter == batchCounter * batch)
                    {
                        if (singleHeader)
                            result = result.Remove(result.Length - 3, 1);
                        result.AppendLine(bottom);
                        newBatch = true;
                        batchCounter++;
                    }
                } 
            }

            if (lineCounter != batchCounter * batch)
            {
                if (singleHeader)
                    result = result.Remove(result.Length - 3, 1);
                result.AppendLine(bottom);
            }

            if (OperationType != OperationTypes.BlackListBulkUpload)
            {
                result.AppendLine();
                result.AppendLine("COMMIT TRANSACTION");
                result.AppendLine("END TRY");
                result.AppendLine("BEGIN CATCH");
                result.AppendLine("IF @@TRANCOUNT > 0");
                result.AppendLine("ROLLBACK TRANSACTION");
                result.AppendLine("SELECT ERROR_NUMBER() as ErrorNumber, ERROR_MESSAGE() as ErrorMessage, ERROR_LINE() as ErrorLine");
                result.AppendLine("END CATCH");
                result.AppendLine("GO");
            }
            
            using (var writer = new StreamWriter(File.OpenWrite(filename), Encoding.UTF8))
            {
                writer.Write(result);
            }

        }

        private static void LoadFile()
        {
            Accounts = new Dictionary<string, string>();
            BlackListValues = new List<Tuple<string, string>>();
            AccountSegmentValues = new List<Tuple<string, string, string, string>>();
            ILOnlyLimitsValues = new List<Tuple<string, string, string>>();


            while (true)
            {
                Console.WriteLine("Account segmentation (S), WL insert (I), WL update (U), WL removal (R), CL update Payments (P), CL update Risk (C), BlackList bulk upload (B), BlackList upsert (L)?");
                var seg = Console.ReadKey();

                switch (seg.Key)
                {
                    case ConsoleKey.S:
                        OperationType = OperationTypes.Resegmentation;
                        break;
                    case ConsoleKey.Q:
                        OperationType = OperationTypes.PricingManualOverride;
                        break;
                    case ConsoleKey.I:
                        OperationType = OperationTypes.RecalculateLimits;
                        break;
                    case ConsoleKey.T:
                        OperationType = OperationTypes.RecalculateLimitsIL;
                        break;
                    case ConsoleKey.U:
                        OperationType = OperationTypes.WhitelistUpdate;
                        break;
                    case ConsoleKey.R:
                        OperationType = OperationTypes.WhitelistRemoval;
                        break;
                    case ConsoleKey.P:
                        OperationType = OperationTypes.CreditLimitUpdatePayments;
                        break;
                    case ConsoleKey.C:
                        OperationType = OperationTypes.CreditLimitUpdateRisk;
                        break;
                    case ConsoleKey.B:
                        OperationType = OperationTypes.BlackListBulkUpload;
                        break;
                    case ConsoleKey.L:
                        OperationType = OperationTypes.BlackListUpsert;
                        break;
                    case ConsoleKey.A:
                        OperationType = OperationTypes.AccountLock;
                        break;
                    case ConsoleKey.E:
                        OperationType = OperationTypes.Select;
                        break;
                    case ConsoleKey.K:
                        OperationType = OperationTypes.MarkBikError;
                        break;
                    case ConsoleKey.N:
                        OperationType = OperationTypes.UpdateIncome;
                        break;
                }

                if (OperationType != OperationTypes.None)
                    break;
            }
            
            Console.WriteLine("Select file to parse: ");
            var filename = Console.ReadLine();
            if (!filename.EndsWith(".csv"))
            {
                filename += ".csv";
            }

            try
            {
                using (var reader = new StreamReader(File.OpenRead(filename), Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Split(',');

                        if (OperationType == OperationTypes.BlackListBulkUpload || OperationType == OperationTypes.BlackListUpsert)
                        {
                            var tuple = new Tuple<string, string>(line[0], line[1]);
                            BlackListValues.Add(tuple);
                            continue;
                        }

                        if (OperationType == OperationTypes.PricingManualOverride || OperationType == OperationTypes.RecalculateLimits)
                        {
                            var tuple = new Tuple<string, string, string, string>(line[0], line[1], line[2], line[3]);
                            if (line[0] == "accountid")
                                continue;
                            AccountSegmentValues.Add(tuple);
                            continue;
                        }

                        if (OperationType == OperationTypes.RecalculateLimitsIL)
                        {
                            var tuple = new Tuple<string, string, string>(line[0], line[1], line[2]);
                            if (line[0] == "accountid")
                                continue;
                            ILOnlyLimitsValues.Add(tuple);
                            continue;
                        }

                        if (line[0] == "AccountId")
                            continue;
                        if (line.Count() == 1 && OperationType != OperationTypes.BlackListBulkUpload)
                            Accounts.Add(line[0].Trim(), null);
                        else if (!Accounts.ContainsKey(line[0]) && OperationType != OperationTypes.BlackListBulkUpload)
                        {
                            Accounts.Add(line[0].Trim(), line[1].Trim());
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found!");
                LoadFile();
            }
        }

        public class MyC
        {
            public string MyV { get; set; }
        }

        public class program
        {
            public static void test(MyC a)
            {
                a = new MyC();
                a.MyV = "test";
            }

            public static void main()
            {
                var a = new MyC();
                a.MyV = "main";
                test(a);
                Console.Write(a.MyV); // Что выведется в консоль в этом месте?
            }
        }

    }
}
