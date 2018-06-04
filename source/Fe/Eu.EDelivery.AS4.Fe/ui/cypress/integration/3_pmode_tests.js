describe('pmode tests', () => {
  beforeEach(() => cy.login());

  const withinTab = (x, f) => {
    cy
      .get('li > a[data-toggle=tab]:contains(' + x + ')')
      .click({ force: true });

    cy.get('div[title="' + x + '"]').within(f);
  };

  const testDefaultRetryReliability = (x) => {
    withinTab(x, () => {
      cy.getdatacy('retry.count').should('to.be.disabled');
      cy.getdatacy('retry.interval').should('to.be.disabled');

      cy.getdatacy('retry.isEnabled').check({ force: true });

      cy.getdatacy('retry.count').should('be.enabled');
      cy.getdatacy('retry.interval').should('be.enabled');
      cy.getdatacy('retry.count').should('to.have.value', '4');
      cy.getdatacy('retry.interval').should('to.have.value', '1:00:00:00');
    });
  };

  [
    { path: 'receiving', handling: 'Message handling' },
    { path: 'receiving', handling: 'Exception handling' },
    { path: 'sending', handling: 'Receipt handling' },
    { path: 'sending', handling: 'Error handling' },
    { path: 'sending', handling: 'Exception handling' }
  ].forEach((x) => {
    it('enables ' + x.handling + ' retry on enable', () => {
      cy.visit('/pmodes/' + x.path);
      cy.getdatacy('select-pmodes').select('02-sample-pmode', { force: true });

      testDefaultRetryReliability(x.handling);
    });
  });

  it.only('should use the user-specified retry reliability values over the defaults', () => {
    cy.visit('/pmodes/receiving');
    cy.getdatacy('select-pmodes').select('01-sample-pmode', { force: true });

    withinTab('Message handling', () => {
      cy.getdatacy('retry.isEnabled').check({ force: true });
      cy.getdatacy('retry.count').type('{selectall}5');
      cy.getdatacy('retry.interval').type('{selectall}0:00:01:00');
      cy.getdatacy('retry.isEnabled').uncheck({ force: true });
    });

    cy.getdatacy('save').click();
    cy.reload();

    withinTab('Message handling', () => {
      cy.getdatacy('retry.isEnabled').check({ force: true });
      cy.getdatacy('retry.count').should('to.have.value', '5');
      cy.getdatacy('retry.interval').should('to.have.value', '0:00:01:00');
    });
  });
});
