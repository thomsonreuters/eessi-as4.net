import jq from 'jquery';

describe('page smoke tests', () => {
  beforeEach(() => {
    cy.login();
    cy.visit('/settings/portal');
  });

  it('go through all pages', () => {
    cy.fixture('pages').then((json) => {
      json.pages.forEach((page) => {
        cy.log(`go to ${page.title}`);
        if (page.show !== undefined) {
          cy.getdatacy(page.show).click({ force: true });
        }
        cy.getdatacy(page.title).click({ force: true });
        cy.getdatacy('header').should('contain', page.title);
      });
    });
  });
});

describe('send pmodes', () => {
  beforeEach(() => {
    cy.login();
    cy.visit('/pmodes/sending');
  });

  it('loads sample pmodes', () => {
    cy.getdatacy('select-pmodes').within(() => {
      cy.get('option').should('not.be.empty');
    });
  });

  it('rename/delete sample pmode', () => {
    cy.fixture('send_pmode').then((json) => {
      cy.getdatacy('select-pmodes').select(json.originalName);
      cy.getdatacy('rename').click();
      cy.get('div.modal-body input[type=text]').type(
        '{selectall}' + json.newName
      );
      cy.getdatacy('ok').click();
      cy.getdatacy('save').click();
      cy.getdatacy('select-pmodes').select(json.newName);
      cy.getdatacy('delete').click();
      cy.getdatacy('ok').click(); // Dialog: are you sure?
      cy.getdatacy(json.originalName).should('not.exist');
    });
  });
});

describe('monitoring', () => {
  beforeEach(() => cy.login());

  /** @todo how do we select the right:
   * direction, ebmsMessageType, operation, MEP, and pmodes with generated 'multiselect'?
   * cy.getdatacy('direction').select('Outbound')
   * cy.getdatacy('ebmsMessageType').select('Receipt')
   * cy.getdatacy('status').select('Notified')
   * cy.getdatacy('pmodes').select('01-sample-pmode') */

  it('search for messages', () => {
    cy.visit('/monitor/messages');
    cy.fixture('monitor_message_query').then((json) => {
      cy.getdatacy('ebmsMessageId').type(json.ebmsMessageId);
      cy.getdatacy('fromParty').type(json.fromParty);
      cy.getdatacy('advanced').click();
      cy.getdatacy('mpc').type(json.mpc);
      cy.getdatacy('service').type(json.service);
      cy.getdatacy('action').type(json.action);

      cy.server();
      cy.route('/api/monitor/messages?*').as('query');
      cy.getdatacy('search').click();
      cy.wait('@query')
        .its('url')
        .should(
          'contain',
          '/monitor/messages?' + 'direction=0&direction=1&' + jq.param(json)
        );
    });
  });

  it('search for exceptions', () => {
    cy.visit('/monitor/exceptions');
    cy.fixture('monitor_exception_query').then((json) => {
      cy.getdatacy('ebmsRefToMessageId').type(json.ebmsRefToMessageId);

      cy.server();
      cy.route('/api/monitor/exceptions?*').as('query');
      cy.getdatacy('search').click();
      cy.wait('@query')
        .its('url')
        .should(
          'contain',
          '/monitor/exceptions?' +
            'page=1&direction=0&direction=1&' +
            jq.param(json)
        );
    });
  });
});

describe('users', () => {
  beforeEach(() => cy.login());

  it('create new user', () => {
    cy.visit('/users');
    cy.getdatacy('new').click({ force: true });

    const guestUsername = 'guest-' + Math.random();

    cy.getdatacy('username').type(guestUsername);
    cy.getdatacy('ok').click({ force: true });

    cy.getdatacy('roles').select('readonly', { force: true });
    cy.fixture('login').then((json) => {
      cy.getdatacy('password').type(json.password);
    });

    cy.getdatacy('save').click({ force: true });
    cy.getdatacy('users').should('contain', guestUsername);
  });
});
