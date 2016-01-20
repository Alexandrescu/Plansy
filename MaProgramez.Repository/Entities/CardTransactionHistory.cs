using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace MaProgramez.Repository.Entities
{
    public class CardTransactionHistory
    {
        [NotMapped]
        private readonly Dictionary<int, string> _errorCodes = new Dictionary<int, string>()
        {
            {0, "actiunea a fost acceptata"},
            {16 ,"cardul prezinta un risk (ex. card furat) "},
            {17 ,"numarul cardului este incorect"},
            {18 ,"card blocat"},
            {19 ,"cardul este expirat"},
            {20 ,"fonduri insuficiente"},
            {21 ,"numar CVV2 incorect"},
            {22 ,"banca emitenta nu a putut fi contactata; este posibil sa fie o eroare temporara de comunicatie intre banca procesatoare si banca emitenta a cardului"},
            {32 ,"suma este incorecta"},
            {33 ,"moneda este incorecta"},
            {34 ,"tranzactia nu este permisa (cardul nu poate fi folosit online)"},
            {35 ,"tranzactia a fost respinsa; nu sunt oferite informatii suplimentare pe marginea motivatiei"},
            {36 ,"tranzactia a fost respinsa de filtrele antifrauda"},
            {37 ,"tranzactia a fost respinsa (incalcare a legii)"},
            {38 ,"tranzactia a fost respinsa; nu sunt oferite informatii suplimentare pe marginea motivatiei"},
            {39 ,"tranzactia a fost respinsa; autentificare 3D Secure esuata"},
            {48 ,"cerere invalida"},
            {49 ,"nu se poate pre-autoriza decat o tranzactie noua"},
            {50 ,"nu se poate autoriza decat o tranzactie noua"},
            {51 ,"nu se poate anula decat o tranzactie preautorizata"},
            {52 ,"nu se poate postautoriza decat o tranzactie preautorizata"},
            {53 ,"nu se poate credita decat o tranzactie finalizata"},
            {54 ,"suma de creditare este mai mica decat suma tranzactiei autorizate sau post-autorizate"},
            {55 ,"suma tranzactiei de post-autorizare este mai mare decat suma pre-autorizata"},
            {56 ,"cerere duplicata"},
            {99 ,"eroare generala"}
        };

        [Key]
        public int Id { get; set; }

        public int CardTransactionId { get; set; }

        [ForeignKey("CardTransactionId")]
        public virtual CardTransaction CardTransaction { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "xml")]
        public String RequestXmlContent { get; set; }

        [NotMapped]
        public XElement RequestXmlContentWrapper
        {
            get { return XElement.Parse(RequestXmlContent); }
            set { RequestXmlContent = value.ToString(); }
        }

        [Column(TypeName = "xml")]
        public String ResponseXmlContent { get; set; }

        [NotMapped]
        public XElement ResponseXmlContentWrapper
        {
            get { return XElement.Parse(ResponseXmlContent); }
            set { ResponseXmlContent = value.ToString(); }
        }

        public int ErrorCode { get; set; }

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="CardTransactionHistory"/> class.
        /// </summary>
        public CardTransactionHistory()
        {
            this.Date = DateTime.Now;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the error code description.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns></returns>
        public string GetErrorCodeDescription(int errorCode)
        {
            string description;
            _errorCodes.TryGetValue(errorCode, out description);
            return description;
        }

        #endregion
    }

}
