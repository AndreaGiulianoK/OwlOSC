using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OwlOSC
{
	public class Utils
	{
		public static DateTime TimetagToDateTime(UInt64 val)
		{
			if (val == 1)
				return DateTime.Now;

			UInt32 seconds = (UInt32)(val >> 32);
			var time = DateTime.Parse("1900-01-01 00:00:00");
			time = time.AddSeconds(seconds);
			var fraction = TimetagToFraction(val);
			time = time.AddSeconds(fraction);
			return time;
		}

		public static double TimetagToFraction(UInt64 val)
		{
			if (val == 1)
				return 0.0;

			UInt32 seconds = (UInt32)(val & 0x00000000FFFFFFFF);
			double fraction = (double)seconds / (UInt32)(0xFFFFFFFF);
			return fraction;
		}

		public static UInt64 DateTimeToTimetag(DateTime value)
		{
			UInt64 seconds = (UInt32)(value - DateTime.Parse("1900-01-01 00:00:00.000")).TotalSeconds;
			UInt64 fraction = (UInt32)(0xFFFFFFFF * ((double)value.Millisecond / 1000));

			UInt64 output = (seconds << 32) + fraction;
			return output;
		}

		public static int AlignedStringLength(string val)
		{
			int len = val.Length + (4 - val.Length % 4);
			if (len <= val.Length) len += 4;

			return len;
		}


		const string addressPattern = @"^\/$|^\/([a-zA-Z0-9\/\*\[\]-]*)([a-zA-Z0-9\*\]])$";

		static Regex validateRegex;
		static Regex wildcardRegex;

		public static bool ValideteAddress(string address){
			if(string.IsNullOrEmpty(address))
				return false;
			if(validateRegex == null){
				validateRegex = new Regex(addressPattern, RegexOptions.Compiled);
			}
			var match = validateRegex.IsMatch(address);
			return match;
		}

		public static bool MatchAddress(string address, string prefix){
			if(wildcardRegex == null){
				wildcardRegex = new Regex("\\*", RegexOptions.Compiled);
			}
			bool wildAddress = wildcardRegex.IsMatch(address);
			bool wildPrefix = wildcardRegex.IsMatch(prefix);
			if(!wildAddress && !wildPrefix){
				return (address == prefix);
			}else{
				bool matchAddress = false;
				if(wildAddress){
					string pattern = MakePattern(address);
					matchAddress = Regex.IsMatch(prefix,pattern);
					//Console.WriteLine($"{address} {prefix} -> {pattern} | {matchAddress}");
				}
				bool matchPrefix = false;
				if(wildPrefix){
					string pattern = MakePattern(prefix);
					matchPrefix = Regex.IsMatch(address,pattern);
					//Console.WriteLine($"{prefix} {address} -> {pattern} | {matchPrefix}");
				}
				return matchAddress || matchPrefix;
			}
		}

		private static string MakePattern(string address){
			string[] subs = address.Split('/');
			string pattern = "";
			for (int i=1; i<subs.Length; i++){
				if(i==1 && subs[i] != "*"){
					pattern = $"^";
				}
				if(subs[i] == "*" && i != subs.Length-1){
						pattern += "([a-zA-Z-0-9]*)";
				}else{
					if(i==1)
						pattern += $"(\\/{subs[i]}\\/)";
					else
						pattern += $"(\\/{subs[i]})";
				}
				if(i == subs.Length-1)
					pattern += "([a-zA-Z0-9\\/]*)$";
			}
			return pattern;
		}
	}
}
