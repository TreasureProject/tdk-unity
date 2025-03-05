mergeInto(LibraryManager.library, {

  WebGLNotifyReady: function () {
    window.dispatchEvent(new CustomEvent("tdkReady"))
  },

  WebGLOpenConnectModal: function () {
    window.dispatchEvent(new CustomEvent("tdkOpenConnectModal"))
  },

  WebGLLogOut: function () {
    window.dispatchEvent(new CustomEvent("tdkLogOut"))
  },

  WebGLRequestReconnect: function () {
    window.dispatchEvent(new CustomEvent("tdkRequestReconnect"))
  },

});