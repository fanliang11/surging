namespace Magnum.Calendar
{
    public class CheckResult 
    {
        private readonly bool _isMatch;
        private readonly string _description;

        public CheckResult() : this(false, "No Holiday Detected")
        {

        }

        public CheckResult(bool isMatch, string description)
        {
            _isMatch = isMatch;
            _description = description;
        }


        public bool IsMatch
        {
            get
            {
                return _isMatch;
            }
        }

        public string Description
        {
            get { return _description; }
        }
    }
}