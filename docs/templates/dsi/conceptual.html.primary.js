// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var common = require('./common.js');

exports.transform = function (model) {
  // get contribution information
  model.sourceurl = common.getViewSourceHref(model, model._gitUrlPattern);

  return model;
}
