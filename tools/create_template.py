from pathlib import Path
import json
from docx import Document
from docx.shared import Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_TAB_ALIGNMENT
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT, WD_ROW_HEIGHT_RULE
from docx.enum.style import WD_STYLE_TYPE
root=Path(__file__).resolve().parents[1]
spec=json.loads((root/'Resources/paper-layout.json').read_text(encoding='utf-8-sig'))
scale=(21/2.54*72)/spec['paperWidth'];inner=(spec['paperWidth']-2*spec['paddingX'])*scale;gap=spec['columnGap']*scale
D=Document();sec=D.sections[0];sec.page_width=Pt(21/2.54*72);sec.page_height=Pt(29.7/2.54*72)
sec.left_margin=sec.right_margin=Pt(spec['paddingX']*scale);sec.top_margin=Pt(spec['paddingTop']*scale);sec.bottom_margin=Pt(spec['paddingBottom']*scale)
bg=OxmlElement('w:background');bg.set(qn('w:color'),spec['background']);D._element.insert(0,bg)
D.settings.element.append(OxmlElement('w:displayBackgroundShape'))
def border(p,edge='bottom',size=6,kind='single',color='333333',space=0):
    pr=p.get_or_add_pPr() if hasattr(p,'get_or_add_pPr') else p
    borders=pr.find(qn('w:pBdr'))
    if borders is None:borders=OxmlElement('w:pBdr');pr.append(borders)
    line=OxmlElement('w:'+edge)
    for k,v in [('val',kind),('sz',str(size)),('color',color),('space',str(space))]:line.set(qn('w:'+k),v)
    borders.append(line)
def style(name,size,font='SimSun',east='宋体',bold=False):
    if name not in D.styles:D.styles.add_style(name,WD_STYLE_TYPE.PARAGRAPH)
    s=D.styles[name];s.font.name=font;s.font.size=Pt(size*scale);s.font.color.rgb=RGBColor.from_string(spec['ink']);s.font.bold=bold;s.font.italic=False
    rf=s.element.get_or_add_rPr().rFonts;rf.set(qn('w:eastAsia'),east)
    for key in list(rf.attrib):
        if key.endswith('Theme'):del rf.attrib[key]
    pf=s.paragraph_format;pf.first_line_indent=Pt(0);pf.space_before=Pt(0);pf.space_after=Pt(0);pf.line_spacing=1.4
    for b in list(s.element.xpath('.//w:pBdr')):b.getparent().remove(b)
    return s
style('Normal',spec['bodySize']);normal=D.styles['Normal'].paragraph_format
normal.line_spacing=Pt(spec['bodySize']*spec['bodyLineHeight']*scale);normal.first_line_indent=Pt(2*spec['bodySize']*scale);normal.space_after=Pt(6*scale);normal.alignment=WD_ALIGN_PARAGRAPH.JUSTIFY
style('Title',spec['mastheadSize'],'STXingkai','华文行楷').paragraph_format.alignment=WD_ALIGN_PARAGRAPH.CENTER
spacing=OxmlElement('w:spacing');spacing.set(qn('w:val'),str(round(4*scale*20)));D.styles['Title'].element.get_or_add_rPr().append(spacing)
style('Heading 1',spec['headlineSize'],'SimHei','黑体',True);pf=D.styles['Heading 1'].paragraph_format;pf.alignment=WD_ALIGN_PARAGRAPH.CENTER;pf.space_before=Pt(18*scale);pf.space_after=Pt(10*scale);pf.line_spacing=1.35
style('Heading 2',spec['articleTitleSize'],'SimHei','黑体',True).paragraph_format.space_after=Pt(8*scale)
style('CPSection',spec['labelSize'],'SimHei','黑体');D.styles['CPSection'].paragraph_format.space_after=Pt(5*scale);border(D.styles['CPSection'].element.get_or_add_pPr(),size=5,color='777268',space=2)
style('CPDeck',spec['deckSize'],'FangSong','仿宋');pf=D.styles['CPDeck'].paragraph_format;pf.first_line_indent=Pt(26*scale);pf.line_spacing=1.6;pf.space_after=Pt(14*scale);border(D.styles['CPDeck'].element.get_or_add_pPr(),size=5,color='8F8B80',space=8)
for name in ['CPSection','Heading 1','Heading 2']:D.styles[name].paragraph_format.keep_with_next=True
for name,size in [('CPKicker',9),('CPDate',10),('CPFooter',9),('CPSide',9),('CPBadge',15)]:style(name,size)
for name in ['CPKicker','CPDate','CPFooter']:
    D.styles[name].paragraph_format.tab_stops.add_tab_stop(Pt(inner),WD_TAB_ALIGNMENT.RIGHT)
D.styles['CPKicker'].paragraph_format.space_after=Pt(5*scale);border(D.styles['CPKicker'].element.get_or_add_pPr(),edge='top',size=6,space=4)
D.styles['CPDate'].paragraph_format.space_before=Pt(6*scale);D.styles['CPDate'].paragraph_format.space_after=Pt(6*scale);border(D.styles['CPDate'].element.get_or_add_pPr(),edge='top',kind='double',size=6,space=4);border(D.styles['CPDate'].element.get_or_add_pPr(),size=6,space=4)
D.styles['CPFooter'].paragraph_format.space_before=Pt(14*scale);border(D.styles['CPFooter'].element.get_or_add_pPr(),edge='top',size=12,space=5)
D.styles['CPSide'].paragraph_format.alignment=WD_ALIGN_PARAGRAPH.CENTER;D.styles['CPBadge'].paragraph_format.alignment=WD_ALIGN_PARAGRAPH.CENTER
style('CPEdition',20,'SimHei','黑体',True).paragraph_format.alignment=WD_ALIGN_PARAGRAPH.CENTER
# Prevent the built-in table styles from replacing the newspaper fonts.
def table(widths,caption):
    t=D.add_table(rows=1,cols=len(widths));t.autofit=False;t.alignment=WD_TABLE_ALIGNMENT.LEFT
    prop=t._tbl.tblPr;cap=OxmlElement('w:tblCaption');cap.set(qn('w:val'),caption);prop.append(cap)
    b=OxmlElement('w:tblBorders')
    for edge in ['top','left','bottom','right','insideH','insideV']:
        el=OxmlElement('w:'+edge);el.set(qn('w:val'),'nil');b.append(el)
    prop.append(b)
    for cell,width,col in zip(t.rows[0].cells,widths,t.columns):
        cell.width=Pt(width);col.width=Pt(width);cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
        m=OxmlElement('w:tcMar')
        for edge in ['top','left','bottom','right']:
            el=OxmlElement('w:'+edge);el.set(qn('w:w'),'0');el.set(qn('w:type'),'dxa');m.append(el)
        cell._tc.get_or_add_tcPr().append(m)
        shade=OxmlElement('w:shd');shade.set(qn('w:fill'),spec['background']);cell._tc.get_or_add_tcPr().append(shade)
    return t
D.add_paragraph('城市新闻\tDAILY NEWS','CPKicker')
head=table([62*scale,inner-124*scale,62*scale],'CityPress.Masthead');head.rows[0].height=Pt(96*scale);head.rows[0].height_rule=WD_ROW_HEIGHT_RULE.AT_LEAST
left,centre,right=head.rows[0].cells
left.paragraphs[0].style='CPEdition';left.paragraphs[0].add_run('综合');left.add_paragraph('城市纪事','CPSide')
centre.paragraphs[0].style='Title';centre.paragraphs[0].add_run('{{报纸名称}}')
right.paragraphs[0].style='CPBadge';right.paragraphs[0].add_run('日报')
right.paragraphs[0].paragraph_format.left_indent=right.paragraphs[0].paragraph_format.right_indent=Pt(8*scale)
for edge in ['top','left','bottom','right']:border(right.paragraphs[0]._p,edge,size=5,space=2)
right.add_paragraph('电子刊','CPSide')
D.add_paragraph('{{刊发日期}}\t综合版 · 01','CPDate')
D.add_paragraph('{{本期头条}}','Heading 1');D.add_paragraph('{{导语}}','CPDeck')
articles=table([(inner+gap)/3]*3,'CityPress.Articles')
indent=OxmlElement('w:tblInd');indent.set(qn('w:w'),'0');indent.set(qn('w:type'),'dxa');articles._tbl.tblPr.append(indent)
for i,cell in enumerate(articles.rows[0].cells):
    cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.TOP
    for edge in ['left','right']:cell._tc.tcPr.find(qn('w:tcMar')).find(qn('w:'+edge)).set(qn('w:w'),str(round(gap/2*20)))
    if i:
        borders=OxmlElement('w:tcBorders');line=OxmlElement('w:left');line.set(qn('w:val'),'single');line.set(qn('w:sz'),'5');line.set(qn('w:color'),'B2ADA0');borders.append(line);cell._tc.tcPr.append(borders)
    cell.paragraphs[0].style='CPSection';cell.paragraphs[0].add_run('{{栏目}}');cell.add_paragraph('{{报道标题}}','Heading 2');cell.add_paragraph('{{新闻正文}}')
D.add_paragraph('{{报纸名称}}\t综合 · 城市新闻','CPFooter')
D.core_properties.title='CityPress 统一报纸版式';D.core_properties.author='CityPress'
D.save(root/'Templates/newspaper.docx');print('Created Word template matching the newspaper UI')
