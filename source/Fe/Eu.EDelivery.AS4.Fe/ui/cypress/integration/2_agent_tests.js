describe('new empty agent', () => {
  beforeEach('login', () => cy.login());

  describe('submit agent', () => {
    beforeEach(() => {
      cy.login();
      cy.visit('/submit');
      cy.wait(5000);
    });

    it.skip('adds submit agent', () => {
      cy.fixture('submit_agent').then((json) => {
        cy.getdatacy('new').click({ force: true });
        cy.getdatacy('agent-name').type(json.originalName);
        cy.getdatacy('ok').click();
        cy.getdatacy('save').click({ force: true });
        cy.getdatacy('ok').click(); // Dialog: only takes effect on restart
        cy.getdatacy('agents').select(json.originalName, { force: true });
      });
    });

    it.skip('renames submit agent', () => {
      cy.fixture('submit_agent').then((json) => {
        cy.getdatacy('agents').select(json.originalName);
        cy.getdatacy('rename').click();
        cy.get('div.modal-body input[type=text]').type(json.newName);
        cy.getdatacy('ok').click();
        cy.getdatacy('save').click({ force: true });
        cy.getdatacy('ok').click(); // Dialog: only takes effect on restart
        cy.getdatacy('agents').select(json.newName);
        cy.getdatacy('delete').click();
        cy.getdatacy('ok').click(); // Dialog: are you sure?
        cy.getdatacy(json.newName).should('not.exist');
      });
    });
  });

  let createEmptyAgent = ({ normalPipelineShould, errorPipelineShould }) => {
    const stubName = 'My new agent';
    cy.getdatacy('new').click();
    cy.getdatacy('agent-name').type(stubName);
    cy.getdatacy('clone').select('Empty');
    cy.getdatacy('ok').click();

    cy.getdatacy('collapse').click();
    cy.getdatacy('normal-pipeline').within(() => {
      cy.getdatacy('step-row').should(normalPipelineShould);
    });
    cy.getdatacy('error-pipeline').within(() => {
      cy.getdatacy('step-row').should(errorPipelineShould);
    });
  };

  it.skip('should have default submit steps', () => {
    cy.visit('/submit');
    createEmptyAgent({
      normalPipelineShould: 'not.be.empty',
      errorPipelineShould: 'not.exist'
    });
  });

  it.skip('should have default receive steps', () => {
    cy.visit('/receive/push');
    createEmptyAgent({
      normalPipelineShould: 'not.be.empty',
      errorPipelineShould: 'not.be.empty'
    });
  });
});
