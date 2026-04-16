import { makeStyles } from '@fluentui/react-components';

export const styles = makeStyles({
  container: {
    padding: '24px',
    maxWidth: '1200px',
    margin: '0 auto'
  },
  content: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px'
  },
  actions: {
    display: 'flex',
    gap: '12px',
    marginTop: '16px'
  },
  preview: {
    marginTop: '24px'
  },
  printPreviewContainer: {
    backgroundColor: '#f5f5f5',
    padding: '16px',
    borderRadius: '8px',
    border: '1px solid #ddd'
  },
  printPreview: {
    backgroundColor: 'white',
    padding: '24px',
    borderRadius: '4px',
    marginTop: '12px',
    minHeight: '300px'
  },
  placeholder: {
    color: '#999',
    fontStyle: 'italic',
    padding: '20px',
    textAlign: 'center',
    backgroundColor: '#fafafa',
    borderRadius: '4px',
    marginTop: '12px'
  }
});
