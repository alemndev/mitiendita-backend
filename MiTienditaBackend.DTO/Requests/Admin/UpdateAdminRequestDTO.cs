﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiTienditaBackend.DTO.Requests.Admin
{
    public class UpdateAdminRequestDTO
    {
        public int AdminId { get; set; }
        public string? Mail { get; set; }

        public string? Password { get; set; }

        public string? PasswordHint { get; set; }
    }
}
