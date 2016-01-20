// Ionic Starter App

// angular.module is a global place for creating, registering and retrieving Angular modules
// 'starter' is the name of this angular module example (also set in a <body> attribute in index.html)
// the 2nd parameter is an array of 'requires'
// 'starter.controllers' is found in controllers.js

var controllers = angular.module('starter.controllers', []);
var appUtilities = angular.module('starter.utilities', ['pascalprecht.translate', 'ngMap','ionic-datepicker']);

var mainApp = angular.module('starter', ['ionic', 'starter.controllers', 'starter.utilities', 'ngIOS9UIWebViewPatch']);