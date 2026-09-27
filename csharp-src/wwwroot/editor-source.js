import { basicSetup } from 'codemirror';
import { EditorView } from '@codemirror/view';
import { StreamLanguage, HighlightStyle, syntaxHighlighting } from '@codemirror/language';
import { csharp } from '@codemirror/legacy-modes/mode/clike';
import { tags } from '@lezer/highlight';
import { autocompletion } from '@codemirror/autocomplete';
import { setDiagnostics } from '@codemirror/lint';
import { undo, redo, indentMore } from '@codemirror/commands';
const words = ['public','private','protected','class','override','new','using','namespace','float','int','string','bool','void','return','if','else','foreach','for','while','static','readonly','List','Math','MainGame','Game','Start','Update','Draw','Input','Graphics'];
const members = { Input:['X','Y','Down','Key'], Graphics:['Clear','Rect','Circle','Text','Sprite'] };
const theme = EditorView.theme({
  '&': { height:'100%', backgroundColor:'#10151f', color:'#dce4f5', fontSize:'14px' },
  '.cm-content': { fontFamily:'ui-monospace, SFMono-Regular, Consolas, monospace', caretColor:'#80e6b9', padding:'16px 0' },
  '.cm-scroller': { overflow:'auto' }, '.cm-gutters':{ backgroundColor:'#10151f',color:'#5d6b83',border:'none',paddingRight:'6px' },
  '.cm-activeLine,.cm-activeLineGutter':{ backgroundColor:'#1a2333' },
  '.cm-selectionBackground':{backgroundColor:'#314369 !important'},
  '.cm-tooltip':{backgroundColor:'#202c40',border:'1px solid #3a4d69'},
  '.cm-cursor':{borderLeftColor:'#80e6b9'},'.cm-panels':{backgroundColor:'#202c40',color:'#dce4f5'}
},{dark:true});
const colors = HighlightStyle.define([
  {tag:tags.keyword,color:'#c5a3ff'},{tag:tags.string,color:'#9addad'},
  {tag:tags.number,color:'#ffcc92'},{tag:tags.comment,color:'#74849c',fontStyle:'italic'},
  {tag:tags.typeName,color:'#76d4dc'},{tag:tags.variableName,color:'#dce4f5'},
  {tag:tags.operator,color:'#9bc4fa'},{tag:tags.definition(tags.variableName),color:'#f1ce91'}
]);
export function makeEditor(parent, onChange) {
  let switching = false;
  const editor = new EditorView({parent, extensions:[basicSetup,StreamLanguage.define(csharp),theme,syntaxHighlighting(colors),
    EditorView.contentAttributes.of({autocorrect:'off',autocapitalize:'off',spellcheck:'false'}),
    autocompletion({override:[context=>{
      const word=context.matchBefore(/[\w.]+/); if(!word||(!context.explicit&&!word.text))return null;
      const parts=word.text.split('.'), known=members[parts[0]];
      return {from:known?word.from+parts[0].length+1:word.from,options:(known||words).map(label=>({label,type:known?'property':'keyword'}))};
    }]}),EditorView.updateListener.of(update=>{if(update.docChanged&&!switching)onChange(update.state.doc.toString());})]});
  return {
    set(code){switching=true;editor.dispatch({changes:{from:0,to:editor.state.doc.length,insert:code},selection:{anchor:0}});switching=false;},
    diagnostics(list){editor.dispatch(setDiagnostics(editor.state,list.map(d=>{const l=editor.state.doc.line(Math.min(Math.max(1,d.line),editor.state.doc.lines));const from=Math.min(l.to,l.from+d.column-1);return {from,to:Math.min(from+1,l.to),severity:d.severity==='Error'?'error':'warning',message:d.code+': '+d.message};})));},
    go(line){const l=editor.state.doc.line(Math.min(Math.max(line,1),editor.state.doc.lines));editor.dispatch({selection:{anchor:l.from},effects:EditorView.scrollIntoView(l.from,{y:'center'})});editor.focus();},
    insert(value){if(value==='undo')undo(editor);else if(value==='redo')redo(editor);else if(value==='tab')indentMore(editor);else editor.dispatch(editor.state.replaceSelection(value));editor.focus();},
    get(){return editor.state.doc.toString();}
  };
}
