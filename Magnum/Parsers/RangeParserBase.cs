// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Parsers
{
	using System.Linq;
	using Monads.Parser;

	public abstract class RangeParserBase<TInput> :
		AbstractCharacterParser<TInput>
	{
		protected RangeParserBase()
		{
			Whitespace = Rep(Char(' ').Or(Char('\t').Or(Char('\n')).Or(Char('\r'))));
			NewLine = Rep(Char('\r').Or(Char('\n')));

			ItemSeparator = from c in Char('-') select c;

			ElementSeparator = from c in Rep(Char(';')) select c;

			ValidChar = (from bs in Char('\\')
			             from ch in Char('\\').Or(Char('\"')).Or(Char('-')).Or(Char('/')).Or(Char('\''))
			             select ch)
				.Or(from ch in Char(char.IsLetterOrDigit) select ch)
				.Or(from ch in Char(' ') select ch);


			Pattern = from c in ValidChar
			          from cs in Rep(ValidChar)
			          select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

			Range = (from rs in ElementSeparator
					 from begin in Pattern
					 from separator in ItemSeparator
					 from end in Pattern where begin == end 
					 select (IRangeElement)new StartsWithElement(begin))
				.Or(from rs in ElementSeparator
			        from begin in Pattern
			        from separator in ItemSeparator
			        from end in Pattern
			        select (IRangeElement) new RangeElement(begin, end));

			GreaterThan =  from rs in ElementSeparator
			               from begin in Pattern
			               from separator in ItemSeparator
			               select (IRangeElement) new GreaterThanElement(begin);

			LessThan =     from rs in ElementSeparator
			               from separator in ItemSeparator
						  from end in Pattern
						  select (IRangeElement)new LessThanElement(end);


			StartsWith = (from rs in ElementSeparator
			             from start in Pattern
			             select (IRangeElement) new StartsWithElement(start));

			All = (from r in Range select r)
				.Or(from g in GreaterThan select g)
				.Or(from g in LessThan select g)
				.Or(from s in StartsWith select s);
		}

		public Parser<TInput, char[]> Whitespace { get; private set; }
		public Parser<TInput, char[]> NewLine { get; private set; }

		public Parser<TInput, char> ItemSeparator { get; private set; }
		public Parser<TInput, char[]> ElementSeparator { get; private set; }

		public Parser<TInput, char> ValidChar { get; private set; }
		public Parser<TInput, string> Pattern { get; private set; }

		public Parser<TInput, IRangeElement> Range { get; private set; }
		public Parser<TInput, IRangeElement> GreaterThan { get; private set; }
		public Parser<TInput, IRangeElement> LessThan { get; private set; }
		public Parser<TInput, IRangeElement> StartsWith { get; private set; }

		public Parser<TInput, IRangeElement> All { get; private set; }
	}
}