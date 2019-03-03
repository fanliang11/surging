define(function (require, exports, module) {
    window.debug = false;
    module.exports = {
        GET_ADDRESS: "/ServiceManage/GetAddress",
        GET_SERVICEDESCRIPTOR: "/ServiceManage/GetServiceDescriptor",
        GET_SUBSCRIBERDESCRIPTOR: "/ServiceManage/GetSubscriberDescriptor",
        GET_COMMANDDESCRIPTOR: "/ServiceManage/GetCommandDescriptor",
        GET_SUBSCRIBER: "/ServiceManage/GetSubscriber",
        GET_SERVICECACHE: "/ServiceManage/GetServiceCache",
        EDIT_FAULTTOLERANT: "/ServiceManage/EditFaultTolerant",
        GET_CACHEENDPOINT: "/ServiceManage/GetCacheEndpoint", 
        EDIT_CACHEENDPOINT: "/ServiceManage/EditCacheEndpoint", 
        DEL_CACHEENDPOINT: "/ServiceManage/DelCacheEndpoint", 
        GET_REGISTERADDRESS: "/ServiceManage/GetRegisterAddress"
    }
});
