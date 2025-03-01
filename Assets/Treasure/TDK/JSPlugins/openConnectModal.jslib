mergeInto(LibraryManager.library, {

  OpenConnectModal: function () {
    window.dispatchEvent(new CustomEvent("openConnectModal"))
  },

});