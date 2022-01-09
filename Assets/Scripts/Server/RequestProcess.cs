using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestProcess
{
    private int verificationCode;

    public RequestProcess(int verificationCode)
    {
        this.verificationCode = verificationCode;
    }

    public bool verificationCodeIsValid(int verificationCode)
    {
        return this.verificationCode == verificationCode;
    }

}
