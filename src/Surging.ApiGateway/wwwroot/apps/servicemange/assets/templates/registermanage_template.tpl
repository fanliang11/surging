﻿       {{each Entity}}
	<tr>
                            <td class="center">
								  {{if ($index+1)%2==1 }}
                                  <span class="badge badge-grey ">${$index+1}</span>
                                  {{else}}
                                  <span class="badge   badge-success  ">${$index+1}</span>
                                  {{/if}}
                            </td>
                            <td class="center"> {{= [Address.Ip,Address.Port].join(":")}}</td>
                            <td class="center">已启动</td>
                            <td class="center">
							      {{if IsHealth==true }}
								<span class="label label-success arrowed-in arrowed-in-right">正常</span>
								{{else}}
										<span class="label label-danger arrowed">异常</span>
								{{/if}}
							</td>
                            <td class="center"><div class="visible-md visible-lg hidden-sm hidden-xs action-buttons">
                                                   <a href="/ServiceManage/ServiceManage" nodemenu="" class="grey editRegion" data-pjax=".page-content"><i class="icon-lightbulb  bigger-120"></i>查看服务</a>
											
                                              

                             </div></td>
                           
                        </tr>
						  {{/each}}