
var serviceBase = 'http://api.plansy.nl/';
mainApp.constant('appAuthSettings', {
    apiServiceBaseUri: serviceBase,
    clientId: 'maprogramezMobileApp',
    useRefreshTokens: true,
    logosPath: 'https://maprogramez.net/Images/Logos/'
});

mainApp.constant('$ionicLoadingConfig', {
    template: '<ion-spinner icon="crescent" class="spinner-stable"></ion-spinner>'
});