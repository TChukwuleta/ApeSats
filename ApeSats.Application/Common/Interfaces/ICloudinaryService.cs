﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> FromBase64ToFile(string base64File, string filename);
        Task<string> UploadImage(string base64string, string userid);
        Task<string> UploadInvoiceQRCode(int artId, string userid);
    }
}
