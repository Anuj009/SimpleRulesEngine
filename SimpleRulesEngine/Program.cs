using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRulesEngine
{
    public class Data
    {
        public string Signal { get; set; }
        public dynamic Value { get; set; }
        public string ValueType { get; set; }

        public void setData(string signal, dynamic value, string valueType)
        {
            Signal = signal;
            Value = value;
            ValueType = valueType;
        }
    }

    public class Rules
    {
        private string Signal;
        private dynamic Value;
        private ExpressionType Operator;

        public Rules(string Signal, ExpressionType Operator, dynamic Value)
        {
            this.Signal = Signal;
            this.Operator = Operator;
            this.Value = Value;
        }

        public bool ValidateRule(Data data)
        {
            bool output = true;
            try
            {
                if (data.Signal == Signal)
                {
                    if (data.ValueType == "Integer")
                    {
                        int result = (int)(double.Parse(data.Value.ToString()).CompareTo(Value));

                        if (Operator == ExpressionType.GreaterThanOrEqual)
                            output = (result >= 0) ? true : false;
                        else if (Operator == ExpressionType.GreaterThan)
                            output = (result > 0) ? true : false;
                        else if (Operator == ExpressionType.Equal)
                            output = (result == 0) ? true : false;
                        else if (Operator == ExpressionType.LessThan)
                            output = (result < 0) ? true : false;
                        else if (Operator == ExpressionType.LessThanOrEqual)
                            output = (result <= 0) ? true : false;
                    }
                    else if (data.ValueType == "Datetime")
                    {
                        int result = DateTime.Compare(DateTime.Parse(data.Value.ToString()), Value);

                        if (Operator == ExpressionType.GreaterThanOrEqual)
                            output = (result >= 0) ? true : false;
                        else if (Operator == ExpressionType.GreaterThan)
                            output = (result > 0) ? true : false;
                        else if (Operator == ExpressionType.Equal)
                            output = (result == 0) ? true : false;
                        else if (Operator == ExpressionType.LessThan)
                            output = (result < 0) ? true : false;
                        else if (Operator == ExpressionType.LessThanOrEqual)
                            output = (result <= 0) ? true : false;
                    }
                    else
                    {
                        int result = string.Compare(data.Value, Value);
                        if (Operator == ExpressionType.Equal)
                            output = (result == 0) ? true : false;
                        else if (Operator == ExpressionType.NotEqual)
                            output = (result != 0) ? true : false;
                    }
                }
            }
            catch(Exception e)
            {
                return true;
            }
            return output;
        }
    }

    class Program
    {
        public Dictionary<string, ExpressionType> operators = new Dictionary<string, ExpressionType>()
        {
            { "==",  ExpressionType.Equal },
            { "!=", ExpressionType.NotEqual },
            { "<=", ExpressionType.LessThanOrEqual },
            { ">=", ExpressionType.GreaterThanOrEqual },
            { ">", ExpressionType.GreaterThan },
            { "<", ExpressionType.LessThan }
        };

        static void Main(string[] args)
        {
            string filePath_Rules = "rules.json";
            string filePath_Input = "raw_data.json";
            Data data = new Data();

            // Add new rules below
            List<Rules> rules = new List<Rules>()
            {
                new Rules("ATL1",ExpressionType.LessThanOrEqual, 20),
                new Rules("ATL2",ExpressionType.NotEqual, "LOW"),
                new Rules("ATL3",ExpressionType.LessThanOrEqual, DateTime.Now),
            };

            JArray input = JArray.Parse(File.ReadAllText(filePath_Input));

            //JArray rulesFromFile = JArray.Parse(File.ReadAllText(filePath_Rules));
            //foreach (JObject jItem in rulesFromFile)
            //{
            //    Console.WriteLine(jItem);
            //    Console.WriteLine(jItem.GetValue("signal"));
            //    Console.Read();
            //}

            foreach (JObject jItem in input)
            {
                string valueType = jItem.GetValue("value_type").ToString();
                switch (valueType)
                {
                    case "Integer":
                        data.setData(jItem.GetValue("signal").ToString(), double.Parse(jItem.GetValue("value").ToString()), valueType);
                        break;
                    case "Datetime":
                        data.setData(jItem.GetValue("signal").ToString(), DateTime.Parse(jItem.GetValue("value").ToString()), valueType);
                        break;
                    default:
                        data.setData(jItem.GetValue("signal").ToString(), jItem.GetValue("value").ToString(), valueType);
                        break;
                }
                rules.ForEach(rule =>
                {
                    if (!rule.ValidateRule(data))
                    {
                        Console.WriteLine("The following input fails for one or more rules:\n {0}\n{1}", data.Signal, data.Value);
                        Console.Read();
                    }
                });
            }
            Console.Read();
        }
    }
}
