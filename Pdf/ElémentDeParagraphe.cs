using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace KalosfideAPI.Pdf
{
    public interface ElémentDeParagraphe
    {
        void AjouteA(Paragraph paragraphe);
    }
}
