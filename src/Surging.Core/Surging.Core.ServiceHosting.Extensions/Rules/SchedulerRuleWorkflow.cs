using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Extensions.Rules
{
    public class SchedulerRuleWorkflow
    {
        public SchedulerRuleWorkflow() : this("SchedulerRuleWorkflow", "SchedulerRule", "1 == 1", "OutputExpression", new Dictionary<string, object>())
        {

        }

        public SchedulerRuleWorkflow(string actionExpression) : this()
        {
            var str = Regex.Replace(actionExpression, @"(\.When\()[.\r|\n|\t|\s]*?(?=(function))", ".When(\"", RegexOptions.IgnoreCase);
              str = Regex.Replace(str, @"(\.Skip\()[.\r|\n|\t|\s]*?(?=(function))", ".Skip(\"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(})[.\r|\n|\t|\s]*?(?=(\)))", "}\"", RegexOptions.IgnoreCase);
            Context.Add("expression", str ?? "1 == 1");
        }

        public SchedulerRuleWorkflow(string workflowName, string ruleName, string expression, string ruleActionName, Dictionary<string, object> context)
        {
            WorkflowName = workflowName;
            RuleName = ruleName;
            Expression = expression;
            RuleActionName = ruleActionName;
            Context = context;
        }
        public string WorkflowName { get; set; }

        public string RuleName { get; set; }


        public string Expression { get; set; }

        public string RuleActionName { get; set; }

        public new Dictionary<string, object> Context { get; set; }

        public Workflow GetWorkflow()
        {
            var result = new Workflow
            {
                WorkflowName = WorkflowName,

                Rules = new List<Rule>{
                    new Rule{
                        RuleName =RuleName,
                        Expression = Expression,
                        Actions = new RuleActions{
                            OnSuccess = new ActionInfo{
                                Name = RuleActionName,
                                Context =Context
                            }
                        }
                    }
                }
            };
            return result;
        }
    }
}
