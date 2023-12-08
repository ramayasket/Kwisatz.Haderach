using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kw.Common.Wording
{
    /// <summary> Класс с данными строкового представления числа. </summary>
    internal sealed class AmountInWords
    {
        static readonly object _syncCurrencyRoot = new object();
        static readonly object _syncUnitRoot = new object();
        static Dictionary<string, UnitInfo> _currencies;
        static Dictionary<Units, UnitInfo> _units;

        readonly List<string> _result = new List<string>();

        long _integralPart;
        long _fractalPart;
        UnitInfo _currencyInfo;

        /// <summary> Инициализирует новый экземпляр класса <see cref="AmountInWords"/>. </summary>
        /// <param name="val"> Число для представления. </param>
        /// <param name="currencyCode"> Код валюты. </param>
        internal AmountInWords(decimal val, string currencyCode)
        {
            Init(val, currencyCode);
        }

        /// <summary> Инициализирует новый экземпляр класса <see cref="AmountInWords"/>. </summary>
        /// <param name="val"> Число для представления. </param>
        /// <param name="unit"> Единицы измерения. </param>
        internal AmountInWords(long val, Units unit)
        {
            _currencyInfo = GetUnitInfo(unit);
            _integralPart = val;
            _fractalPart = 0;
        }

        internal AmountInWords IntegralInWords()
        {
            _result.Add(IntegralInWords(_integralPart, _currencyInfo));
            return this;
        }

        internal AmountInWords FractalInNumbers()
        {
            if (_currencyInfo.Precision > 0)
            {
                _result.Add($"{_fractalPart.ToString($"D{_currencyInfo.Precision}")}");
            }

            return this;
        }

        internal AmountInWords IntegralUnitName()
        {
            _result.Add(RusNumber.Case(_integralPart, _currencyInfo.MajorOne, _currencyInfo.MajorTwo, _currencyInfo.MajorFive));
            return this;
        }

        internal AmountInWords FractalUnitName()
        {
            if (_currencyInfo.Precision > 0)
            {
                _result.Add(RusNumber.Case(_fractalPart, _currencyInfo.MinorOne, _currencyInfo.MinorTwo, _currencyInfo.MinorFive));
            }

            return this;
        }

        internal AmountInWords InNumbers()
        {
            _result.Add($"{_integralPart}");
            if (_currencyInfo.Precision > 0)
            {
                _result.Add(".");
                FractalInNumbers();
            }

            return this;
        }

        internal AmountInWords Add(string str)
        {
            _result.Add(str);
            return this;
        }

        /// <summary> Преобразование в строку. </summary>
        /// <returns> Полученная строка. </returns>
        public override string ToString()
        {
            string result = string.Join(string.Empty, _result);
            _result.Clear();
            return result;
        }

        /// <summary> Неявное преобразование в строку. </summary>
        /// <param name="amountInWords"> Класс с данными строкового представления. </param>
        public static implicit operator string(AmountInWords amountInWords) => amountInWords.ToString();

        UnitInfo GetCurrencyInfo(string currency)
        {
            if (!GetCurrencyInfos().ContainsKey(currency))
                throw new ArgumentOutOfRangeException(nameof(currency), "‚Валюта \"" + currency + "\" не зарегистрирована");
            return _currencies[currency];
        }

        UnitInfo GetUnitInfo(Units unit)
        {
            if (!GetUnitInfos().ContainsKey(unit))
                throw new ArgumentOutOfRangeException(nameof(unit), "Единица измерения \"" + unit + "\" не зарегистрирована");
            return _units[unit];
        }

        void Init(decimal val, string currencyCode)
        {
            _currencyInfo = GetCurrencyInfo(currencyCode);
            Init(val);
        }

        void Init(decimal val, UnitInfo currency)
        {
            _currencyInfo = currency;
            Init(val);
        }

        void Init(decimal val)
        {
            val = Math.Abs(val);
            val = Math.Round(val, _currencyInfo.Precision, MidpointRounding.ToEven);
            decimal decIntegral = decimal.Truncate(val);
            decimal decFractal = (val - decIntegral) * (decimal)Math.Pow(10, _currencyInfo.Precision);
            _fractalPart = (long)decFractal;
            _integralPart = (long)decIntegral;
        }

        static string IntegralInWords(long val, UnitInfo info)
        {
            var r = new StringBuilder();
            if (val == 0)
                r.Append("ноль ");
            if (val % 1000 != 0)
                r.Append(RusNumber.Str(val, info.Gender, string.Empty, string.Empty, string.Empty));
            val /= 1000;
            r.Insert(0, RusNumber.Str(val, NounGender.Feminine, "тысяча", "тысячи", "тысяч"));
            val /= 1000;
            r.Insert(0, RusNumber.Str(val, NounGender.Masculine, "миллион", "миллиона", "миллионов"));
            val /= 1000;
            r.Insert(0, RusNumber.Str(val, NounGender.Masculine, "миллиард", "миллиарда", "миллиардов"));
            val /= 1000;
            r.Insert(0, RusNumber.Str(val, NounGender.Masculine, "триллион", "триллиона", "триллионов"));
            val /= 1000;
            r.Insert(0, RusNumber.Str(val, NounGender.Masculine, "триллиард", "триллиарда", "триллиардов"));
            r[0] = char.ToUpper(r[0]);
            return r.ToString().Trim();
        }

        Dictionary<string, UnitInfo> GetCurrencyInfos()
        {
            if (_currencies == null)
            {
                lock (_syncCurrencyRoot)
                {
                    if (_currencies == null)
                    {
                        var temp = LoadCurrencyInfos();
                        Interlocked.CompareExchange(ref _currencies, temp, null);
                    }
                }
            }

            return _currencies;
        }

        /// <summary> Получить информацию о единицах измерения. </summary>
        Dictionary<Units, UnitInfo> GetUnitInfos()
        {
            if (_units == null)
            {
                lock (_syncUnitRoot)
                {
                    if (_units == null)
                    {
                        var temp = new Dictionary<Units, UnitInfo>
                        {
                            { Units.RuDay, new UnitInfo { MajorOne = "день", MajorTwo = "дня", MajorFive = "дней", Gender = NounGender.Masculine } },
                            { Units.RuMonth, new UnitInfo { MajorOne = "месяц", MajorTwo = "месяца", MajorFive = "месяцев", Gender = NounGender.Masculine } },
                            { Units.RuYear, new UnitInfo { MajorOne = "год", MajorTwo = "года", MajorFive = "лет", Gender = NounGender.Masculine } },
                            { Units.EnDay, new UnitInfo { MajorOne = "day", MajorTwo = "days", MajorFive = "days" } },
                            { Units.EnMonth, new UnitInfo { MajorOne = "month", MajorTwo = "months", MajorFive = "months" } },
                            { Units.EnYear, new UnitInfo { MajorOne = "year", MajorTwo = "years", MajorFive = "years" } },
                            { Units.Percent, new UnitInfo { MajorOne = "процент", MajorTwo = "процента", MajorFive = "процентов" } },
                        };
                        Interlocked.Exchange(ref _units, temp);
                    }
                }
            }

            return _units;
        }

        /// <summary> Получить настройки валют. </summary>
        /// <returns> Настройки валют. </returns>
        Dictionary<string, UnitInfo> LoadCurrencyInfos()
        {
            return new Dictionary<string, UnitInfo>
            {
                {
                    "RUR",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "рубль",
                        MajorTwo = "рубля",
                        MajorFive = "рублей",
                        MinorOne = "копейка",
                        MinorTwo = "копейки",
                        MinorFive = "копеек",
                        Precision = 2,
                    }
                },
                {
                    "RUB",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "рубль",
                        MajorTwo = "рубля",
                        MajorFive = "рублей",
                        MinorOne = "копейка",
                        MinorTwo = "копейки",
                        MinorFive = "копеек",
                        Precision = 2,
                    }
                },
                {
                    "USD",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "доллар США",
                        MajorTwo = "доллара США",
                        MajorFive = "долларов США",
                        MinorOne = "цент",
                        MinorTwo = "цента",
                        MinorFive = "центов",
                        Precision = 2,
                    }
                },
                {
                    "EUR",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "евро",
                        MajorTwo = "евро",
                        MajorFive = "евро",
                        MinorOne = "цент",
                        MinorTwo = "цента",
                        MinorFive = "центов",
                        Precision = 2,
                    }
                },
                {
                    "AUD",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "Австралийский доллар",
                        MajorTwo = "Австралийских доллара",
                        MajorFive = "Австралийских долларов",
                        MinorOne = "цент",
                        MinorTwo = "цента",
                        MinorFive = "центов",
                        Precision = 2,
                    }
                },
                {
                    "CAD",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "Канадский доллар",
                        MajorTwo = "Канадских доллара",
                        MajorFive = "Канадских долларов",
                        MinorOne = "цент",
                        MinorTwo = "цента",
                        MinorFive = "центов",
                        Precision = 2,
                    }
                },
                {
                    "CNY",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "юань",
                        MajorTwo = "юаня",
                        MajorFive = "юаней",
                        MinorOne = "фэнь",
                        MinorTwo = "фэня",
                        MinorFive = "фэней",
                        Precision = 2,
                    }
                },
                {
                    "JPY",
                    new UnitInfo
                    {
                        Gender = NounGender.Feminine,
                        MajorOne = "йена",
                        MajorTwo = "йены",
                        MajorFive = "йен",
                        MinorOne = "сен",
                        MinorTwo = "сена",
                        MinorFive = "сенов",
                        Precision = 2,
                    }
                },
                {
                    "CHF",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "Швейцарский франк",
                        MajorTwo = "Швейцарских франка",
                        MajorFive = "Швейцарских франков",
                        MinorOne = "раппен",
                        MinorTwo = "раппена",
                        MinorFive = "раппенов",
                        Precision = 2,
                    }
                },
                {
                    "GBP",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "фунт стерлингов",
                        MajorTwo = "фунта стерлингов",
                        MajorFive = "фунтов стерлингов",
                        MinorOne = "пенни",
                        MinorTwo = "пенни",
                        MinorFive = "пенни",
                        Precision = 2,
                    }
                },
                {
                    "PLN",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "злотый",
                        MajorTwo = "злотых",
                        MajorFive = "злотых",
                        MinorOne = "грош",
                        MinorTwo = "гроша",
                        MinorFive = "грошей",
                        Precision = 2,
                    }
                },
                {
                    "NOK",
                    new UnitInfo
                    {
                        Gender = NounGender.Feminine,
                        MajorOne = "Норвежская крона",
                        MajorTwo = "Норвежских кроны",
                        MajorFive = "Норвежских крон",
                        MinorOne = "эре",
                        MinorTwo = "эре",
                        MinorFive = "эре",
                        Precision = 2,
                    }
                },
                {
                    "SEK",
                    new UnitInfo
                    {
                        Gender = NounGender.Feminine,
                        MajorOne = "Шведская крона",
                        MajorTwo = "Шведских кроны",
                        MajorFive = "Шведских крон",
                        MinorOne = "эре",
                        MinorTwo = "эре",
                        MinorFive = "эре",
                        Precision = 2,
                    }
                },
                {
                    "KZT",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "тенге",
                        MajorTwo = "тенге",
                        MajorFive = "тенге",
                        MinorOne = "тиын",
                        MinorTwo = "тиына",
                        MinorFive = "тиынов",
                        Precision = 2,
                    }
                },
                {
                    "CZK",
                    new UnitInfo
                    {
                        Gender = NounGender.Feminine,
                        MajorOne = "Чешская крона",
                        MajorTwo = "Чешские кроны",
                        MajorFive = "Чешских крон",
                        MinorOne = "геллер",
                        MinorTwo = "геллера",
                        MinorFive = "геллеров",
                        Precision = 2,
                    }
                },
                {
                    "SGD",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "Сингапурский доллар",
                        MajorTwo = "Сингапурских доллара",
                        MajorFive = "Сингапурских долларов",
                        MinorOne = "цент",
                        MinorTwo = "цента",
                        MinorFive = "центов",
                        Precision = 2,
                    }
                },
                {
                    "HKD",
                    new UnitInfo
                    {
                        Gender = NounGender.Masculine,
                        MajorOne = "Гонконгский доллар",
                        MajorTwo = "Гонконгских доллара",
                        MajorFive = "Гонконгских долларов",
                        MinorOne = "цент",
                        MinorTwo = "цента",
                        MinorFive = "центов",
                        Precision = 2,
                    }
                },
            };
        }
    }
}
