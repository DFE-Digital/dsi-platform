// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var common = require('./ManagedReference.common.js');
var overwrite = require('./ManagedReference.overwrite.js');

exports.transform = function (model) {
  model.yamlmime = "ManagedReference";

  if (overwrite && overwrite.transform) {
    return overwrite.transform(model);
  }

  if (common && common.transform) {
    model = common.transform(model);
  }
  if (model.type.toLowerCase() === "enum") {
    model.isClass = false;
    model.isEnum = true;
  }
  model._disableNextArticle = true;

  if (model._splitReference) {
    model = postTransformMemberPage(model);
  }

  if (model.type === "class") {
    model.interactionResponse = (model.attributes ?? [])
      .filter(attribute => attribute.type === 'Dfe.SignIn.Base.Framework.AssociatedResponseAttribute')
      .map(attribute => attribute.arguments[0].value);

    model.interactionExceptions = (model.attributes ?? [])
      .filter(attribute => attribute.type === 'Dfe.SignIn.Base.Framework.ThrowsAttribute')
      .map(attribute => attribute.arguments[0].value);

    model.hasInheritanceSection = (model.inheritance?.length || 0) > 1
      || (model.derivedClasses?.length || 0) !== 0;
  }

  if (["field", "property", "event"].includes(model.type)) {
    model.hideHeading = true;
  }

  return model;
}

exports.getOptions = function (model) {
  if (overwrite && overwrite.getOptions) {
    return overwrite.getOptions(model);
  }
  var ignoreChildrenBookmarks = model._splitReference && model.type && common.getCategory(model.type) === 'ns';

  return {
    // "bookmarks": common.getBookmarks(model, ignoreChildrenBookmarks)
  };
}

function postTransformMemberPage(model) {
  var type = model.type.toLowerCase();
  var category = common.getCategory(type);
  if (category == 'class') {
    var typePropertyName = common.getTypePropertyName(type);
    if (typePropertyName) {
      model[typePropertyName] = true;
    }
    if (model.children && model.children.length > 0) {
      model.isCollection = true;
      common.groupChildren(model, 'class');
    } else {
      model.isItem = true;
    }
  }
  return model;
}
