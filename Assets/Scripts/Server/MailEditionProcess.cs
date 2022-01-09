using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailEditionProcess  : RequestProcess
{
    private string newMail;
    
    public MailEditionProcess(string newMail, int verificationCode) : base(verificationCode)
    {
        this.newMail = newMail;
    }

    public string getNewMail()
    {
        return newMail;
    }

}
