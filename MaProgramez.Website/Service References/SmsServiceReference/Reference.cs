﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MaProgramez.Website.SmsServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="SmsServiceReference.ISmsWcfService")]
    public interface ISmsWcfService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISmsWcfService/SendSms", ReplyAction="http://tempuri.org/ISmsWcfService/SendSmsResponse")]
        bool SendSms(string sourceId, string recipientNumber, string smsContent, System.Nullable<System.DateTime> scheduledDateTime);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISmsWcfService/SendSms", ReplyAction="http://tempuri.org/ISmsWcfService/SendSmsResponse")]
        System.Threading.Tasks.Task<bool> SendSmsAsync(string sourceId, string recipientNumber, string smsContent, System.Nullable<System.DateTime> scheduledDateTime);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISmsWcfServiceChannel : MaProgramez.Website.SmsServiceReference.ISmsWcfService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SmsWcfServiceClient : System.ServiceModel.ClientBase<MaProgramez.Website.SmsServiceReference.ISmsWcfService>, MaProgramez.Website.SmsServiceReference.ISmsWcfService
    {
        
        public SmsWcfServiceClient() {
        }
        
        public SmsWcfServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SmsWcfServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SmsWcfServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SmsWcfServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool SendSms(string sourceId, string recipientNumber, string smsContent, System.Nullable<System.DateTime> scheduledDateTime) {
            return base.Channel.SendSms(sourceId, recipientNumber, smsContent, scheduledDateTime);
        }
        
        public System.Threading.Tasks.Task<bool> SendSmsAsync(string sourceId, string recipientNumber, string smsContent, System.Nullable<System.DateTime> scheduledDateTime) {
            return base.Channel.SendSmsAsync(sourceId, recipientNumber, smsContent, scheduledDateTime);
        }
    }
}