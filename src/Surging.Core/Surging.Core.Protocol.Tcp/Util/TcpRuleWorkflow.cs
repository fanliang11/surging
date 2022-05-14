using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Util
{
    public class TcpRuleWorkflow
    {

        public TcpRuleWorkflow() : this("TcpRuleWorkflow", "TcpRule", "1 == 1", "OutputExpression", new Dictionary<string, object>())
        {

        }

        public TcpRuleWorkflow(string actionExpression) : this()
        {
            Context.Add("expression", actionExpression);
        }

        public TcpRuleWorkflow(string workflowName,string ruleName,string expression,string ruleActionName, Dictionary<string, object> context)
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
