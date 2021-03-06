﻿using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPMeta2.Containers.Assertion;
using SPMeta2.CSOM.ModelHandlers;
using SPMeta2.CSOM.ModelHosts;
using SPMeta2.Definitions;
using SPMeta2.Definitions.Base;
using SPMeta2.Enumerations;
using SPMeta2.Exceptions;
using SPMeta2.Regression.CSOM.Utils;
using SPMeta2.Utils;


namespace SPMeta2.Regression.CSOM.Validation
{
    public class ClientFieldDefinitionValidator : FieldModelHandler
    {
        #region properties

        public bool SkipRequredPropValidation { get; set; }
        public bool SkipDescriptionPropValidation { get; set; }

        #endregion

        public override void DeployModel(object modelHost, DefinitionBase model)
        {
            var definition = model.WithAssertAndCast<FieldDefinition>("model", value => value.RequireNotNull());
            var spObject = GetField(modelHost, definition);

            var assert = ServiceFactory.AssertService.NewAssert(model, definition, spObject);

            ValidateField(assert, spObject, definition);
        }

        protected Field GetField(object modelHost, FieldDefinition definition)
        {
            if (modelHost is SiteModelHost)
                return FindExistingSiteField(modelHost as SiteModelHost, definition);
            else if (modelHost is ListModelHost)
                return FindExistingListField((modelHost as ListModelHost).HostList, definition);
            else
            {
                throw new SPMeta2NotSupportedException(
                    string.Format("Validation for artifact of type [{0}] under model host [{1}] is not supported.",
                    definition.GetType(),
                    modelHost.GetType()));
            }
        }

        protected void ValidateField(AssertPair<FieldDefinition, Field> assert, Field spObject, FieldDefinition definition)
        {
            assert
                .ShouldNotBeNull(spObject)
                .ShouldBeEqual(m => m.Title, o => o.Title)
                    //.ShouldBeEqual(m => m.InternalName, o => o.InternalName)
                    .ShouldBeEqual(m => m.Id, o => o.Id)
                    .ShouldBeEqual(m => m.FieldType, o => o.TypeAsString)
                    .ShouldBeEqual(m => m.Group, o => o.Group);

            // TODO, R&D to check InternalName changes in list-scoped fields
            if (spObject.InternalName == definition.InternalName)
            {
                assert.ShouldBeEqual(m => m.InternalName, o => o.InternalName);
            }
            else
            {
                assert.SkipProperty(m => m.InternalName,
                    "Target InternalName is different to source InternalName. Could be an error if this is not a list scoped field");
            }

            assert.ShouldBeEqual(m => m.ValidationFormula, o => o.ValidationFormula);
            assert.ShouldBeEqual(m => m.ValidationMessage, o => o.ValidationMessage);

            // taxonomy field seems to prodice issues w/ Required/Description validation
            if (!SkipRequredPropValidation)
                assert.ShouldBeEqual(m => m.Required, o => o.Required);
            else
                assert.SkipProperty(m => m.Required, "Skipping Required prop validation.");

            if (!SkipDescriptionPropValidation)
                assert.ShouldBeEqual(m => m.Description, o => o.Description);
            else
                assert.SkipProperty(m => m.Description, "Skipping Description prop validation.");

            assert.ShouldBeEqual(m => m.Hidden, o => o.Hidden);

            if (!string.IsNullOrEmpty(definition.DefaultValue))
                assert.ShouldBePartOf(m => m.DefaultValue, o => o.DefaultValue);
            else
                assert.SkipProperty(m => m.DefaultValue, string.Format("Default value is not set. Skippping."));

            if (!string.IsNullOrEmpty(spObject.JSLink) &&
                (spObject.JSLink == "SP.UI.Taxonomy.js|SP.UI.Rte.js(d)|SP.Taxonomy.js(d)|ScriptForWebTaggingUI.js(d)" ||
                spObject.JSLink == "choicebuttonfieldtemplate.js" ||
                spObject.JSLink == "clienttemplates.js"))
            {
                assert.SkipProperty(m => m.JSLink, string.Format("OOTB read-ony JSLink value:[{0}]", spObject.JSLink));
            }
            else
            {
                assert.ShouldBePartOf(m => m.JSLink, o => o.JSLink);
            }

            if (definition.ShowInDisplayForm.HasValue)
                assert.ShouldBeEqual(m => m.ShowInDisplayForm, o => o.GetShowInDisplayForm());
            else
                assert.SkipProperty(m => m.ShowInDisplayForm, "ShowInDisplayForm is NULL");

            if (definition.ShowInEditForm.HasValue)
                assert.ShouldBeEqual(m => m.ShowInEditForm, o => o.GetShowInEditForm());
            else
                assert.SkipProperty(m => m.ShowInEditForm, "ShowInEditForm is NULL");

            if (definition.ShowInListSettings.HasValue)
                assert.ShouldBeEqual(m => m.ShowInListSettings, o => o.GetShowInListSettings());
            else
                assert.SkipProperty(m => m.ShowInListSettings, "ShowInListSettings is NULL");

            if (definition.ShowInNewForm.HasValue)
                assert.ShouldBeEqual(m => m.ShowInNewForm, o => o.GetShowInNewForm());
            else
                assert.SkipProperty(m => m.ShowInNewForm, "ShowInNewForm is NULL");

            if (definition.ShowInVersionHistory.HasValue)
                assert.ShouldBeEqual(m => m.ShowInVersionHistory, o => o.GetShowInVersionHistory());
            else
                assert.SkipProperty(m => m.ShowInVersionHistory, "ShowInVersionHistory is NULL");

            if (definition.ShowInViewForms.HasValue)
                assert.ShouldBeEqual(m => m.ShowInViewForms, o => o.GetShowInViewForms());
            else
                assert.SkipProperty(m => m.ShowInViewForms, "ShowInViewForms is NULL");

            assert
                .ShouldBeEqual(m => m.Indexed, o => o.Indexed);

            if (definition.AllowDeletion.HasValue)
                assert.ShouldBeEqual(m => m.AllowDeletion, o => o.GetAllowDeletion());
            else
                assert.SkipProperty(m => m.AllowDeletion, "AllowDeletion is NULL");
        }
    }

}
