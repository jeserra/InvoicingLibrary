﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing
{
    public interface ITimbrado
    {
        string Timbrar(string ComprobanteXML);
        void ObtenerPDF(string TimbradoXML);
    }
}
