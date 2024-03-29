﻿using ApeSats.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common.Interfaces.Validators
{
    public interface IBaseValidator
    {
        public string UserId { get; set; }
    }

    public interface IStatusValidator : IBaseValidator
    {
        public Status Status { get; set; }
    }
    public interface INameValidator : IBaseValidator
    {
        public string Name { get; set; }
    }
    public interface IIdValidator : IBaseValidator
    {
        public int Id { get; set; }
    }
    public interface IUserIdValidator : IBaseValidator
    {
        public string UserId { get; set; }
    }
    public interface IEmailValidator
    {
        public string Email { get; set; }
    }
    public interface IDateFilterValidator : IBaseValidator
    {
        public DateTime Filter { get; set; }
    }
}
